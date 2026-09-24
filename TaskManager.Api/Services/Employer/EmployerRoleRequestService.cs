using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDto.Employer;
using TaskManager.Api.Data.DTO.RequestDto.EmployerDto;
using TaskManager.Api.Data.DTO.RequestDTOs.Employer;
using TaskManager.Api.Data.DTO.ResponseDTOs.Employer;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.Employer;

namespace TaskManager.Api.Services.Employer
{
    public class EmployerRoleRequestService : IEmployerRoleRequestService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmployerRoleRequestService> _logger;

        public EmployerRoleRequestService(AppDbContext db,
            UserManager<ApplicationUser> userManager,
            ILogger<EmployerRoleRequestService> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<BaseResponseDto<List<RequestToEmployerRoleDto>>> GetPendingEmployerRequestsAsync()
        {
            var pendingRequests = await _db.EmployerRequests
                .AsNoTracking()
                .Where(r => r.Status == RequestStatus.Pending)
                .Select(r => new RequestToEmployerRoleDto
                {
                    RequestId = r.Id,
                    Website = r.Website,
                    Description = r.Description,
                    UserId = r.UserId,
                    CompanyName = r.CompanyName,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Status = r.Status
                }).ToListAsync();

            return ResponseFactory.Ok(pendingRequests);
        }

        public async Task<BaseResponseDto<RequestToEmployerRoleDto>> GetPendingRequestByIdAsync(int requestId)
        {
            var request = await _db.EmployerRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == RequestStatus.Pending);
            if (request == null)
            {
                _logger.LogWarning("Pending employer request with ID {RequestId} not found.", requestId);
                return ResponseFactory.Fail<RequestToEmployerRoleDto>(ResponseType.NotFound);
            }

            var requestResponse = MapToEmployerRequestResponse(request);

            return ResponseFactory.Ok(requestResponse);
        }

        public async Task<BaseResponseDto> ApproveEmployerRequestAsync(int requestId, ApproveEmployerRoleRequestDto dto, string adminId)
        {
            var request = await _db.EmployerRequests.FindAsync(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
            {
                return ResponseFactory.Fail(ResponseType.BadRequest, "Request not found or cannot be approved");
            }

            var user = await _db.Users.FindAsync(request.UserId);
            if (user == null)
            {
                var message = "User not found, request is rejected";
                UpdateEmployerRequest(request, adminId, RequestStatus.Rejected, message);
                await _db.SaveChangesAsync();
                return ResponseFactory.Fail(ResponseType.NotFound, message);
            }

            var responseMessage = await UserAlreadyEmployerOrPerformerAsync(user, request, adminId);
            if (responseMessage != null)
            {
                return ResponseFactory.Fail(ResponseType.Conflict, responseMessage);
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            _logger.LogInformation("Starting transaction to approve employer request with ID {RequestId} for user {UserId}.",
                requestId, user.Id);

            try
            {
                var profile = CreateEmployerProfileEntity(user.Id, request);
                await _db.EmployerProfiles.AddAsync(profile);
                _logger.LogInformation("Creating employer profile for user {UserId} with company name {CompanyName}",
                    user.Id, request.CompanyName);

                var addRole = await _userManager.AddToRoleAsync(user, RolesName.Employer);
                if (!addRole.Succeeded)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError("Error assigning Employer role to user {UserId}: {Errors}",
                        user.Id, string.Join(", ", addRole.Errors.Select(e => e.Description)));
                    return ResponseFactory.Fail(ResponseType.InternalServerError, ErrorMessages.ServerError);
                }

                UpdateEmployerRequest(request, adminId, RequestStatus.Approved, dto.AdminComment);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully approved employer request with ID {RequestId} for user {UserId}. Approved by admin: {AdminId}",
                    requestId, user.Id, adminId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while approving employer request with ID {RequestId} for user {UserId}.",
                    requestId, user.Id);
                return ResponseFactory.Fail(ResponseType.InternalServerError, ErrorMessages.ServerError);
            }

            return ResponseFactory.Ok("Request successfully approved");
        }


        public async Task<BaseResponseDto> RejectEmployerRequestAsync(int requestId, RejectEmployerRoleRequestDto dto, string adminId)
        {
            var request = await _db.EmployerRequests.FindAsync(requestId);
            if (request == null || request.Status != RequestStatus.Pending)
            {
                return ResponseFactory.Fail(ResponseType.BadRequest, "Request not found or cannot be approved");
            }

            UpdateEmployerRequest(request, adminId, RequestStatus.Rejected, dto.Reason);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Employer request with ID {RequestId} has been rejected by admin {AdminId}. Reason: {Reason}",
                requestId, adminId, dto.Reason);

            return ResponseFactory.Ok("Request successfully rejected");
        }


        public async Task<BaseResponseDto> CreateEmployerRoleRequestAsync(CreateEmployerRoleRequestDto dto, ApplicationUser user)
        {
            var existPending = await _db.EmployerRequests
                .FirstOrDefaultAsync(r => r.UserId == user.Id);
            if (existPending != null)
            {
                _logger.LogWarning("User {UserName} (ID {UserId}) attempted to submit a request for the employer role," +
                    " however, he had previously submitted an application that was rejected (request ID {RequestId}).",
                    user.UserName, user.Id, existPending.Id);
                return ResponseFactory.Fail(ResponseType.Conflict, "You can submit an application only once.");
            }

            var request = CreateEmployerRequestEntity(dto, user.Id);

            await _db.EmployerRequests.AddAsync(request);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User {UserName} submitted an employer request with ID {RequestId}",
                user.UserName, request.Id);

            return ResponseFactory.Ok($"Your request has been submitted successfully. Request ID: {request.Id}");
        }



        //private methods
        private async Task<string?> UserAlreadyEmployerOrPerformerAsync(ApplicationUser user, EmployerRequest request, string adminId)
        {
            var isEmployer = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (isEmployer)
            {
                var responseMessage = "User already has Employer role — request marked rejected";
                UpdateEmployerRequest(request, adminId, RequestStatus.Rejected, responseMessage);
                await _db.SaveChangesAsync();
                return responseMessage;
            }
            var userPerformingTasks = await _db.TaskPerformers.FirstOrDefaultAsync(tp => tp.PerformerId == user.Id);
            if (userPerformingTasks != null)
            {
                var responseMessage = "The user cannot become an employer while participating in tasks.";
                UpdateEmployerRequest(request, adminId, RequestStatus.Rejected, responseMessage);
                await _db.SaveChangesAsync();
                return responseMessage;
            }
            return null;
        }


        //private static methods
        private static EmployerRequest CreateEmployerRequestEntity(CreateEmployerRoleRequestDto dto, string userId)
        {
            var request = new EmployerRequest
            {
                CompanyName = dto.CompanyName,
                Description = dto.Description,
                Website = dto.Website,
                UserId = userId,
                Status = RequestStatus.Pending
            };
            return request;
        }

        private static void UpdateEmployerRequest(EmployerRequest request, string adminId, RequestStatus status, string adminComment)
        {
            request.Status = status;
            request.AdminComment = adminComment;
            request.ReviewedBy = adminId;
            request.ReviewedAt = DateTimeOffset.UtcNow;
        }

        private static EmployerProfile CreateEmployerProfileEntity(string userId, EmployerRequest request)
        {
            var profile = new EmployerProfile
            {
                EmployerId = userId,
                CompanyName = request.CompanyName,
                Website = request.Website,
                Description = request.Description
            };
            return profile;
        }

        private static RequestToEmployerRoleDto MapToEmployerRequestResponse(EmployerRequest request)
        {
            var requestResponse = new RequestToEmployerRoleDto
            {
                RequestId = request.Id,
                Website = request.Website,
                UserId = request.UserId,
                CompanyName = request.CompanyName,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                Status = request.Status,
                Description = request.Description
            };
            return requestResponse;
        }
    }
}
