using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployerProfileService _profileService;
        private readonly ILogger<AdminService> _logger;

        public AdminService(AppDbContext db, UserManager<ApplicationUser> userManager, EmployerProfileService profileService, ILogger<AdminService> logger)
        {
            _db = db;
            _userManager = userManager;
            _profileService = profileService;
            _logger = logger;
        }


        public async Task<List<EmployerRequestSummaryDto>> GetPendingEmployerRequestsAsync()
        {
            var pendingRequests = await _db.EmployerRequests
                .AsNoTracking()
                .Where(r => r.Status == RequestStatus.Pending)
                .Select(r => new EmployerRequestSummaryDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    CompanyName = r.CompanyName,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Status = r.Status
                }).ToListAsync();

            return pendingRequests;
        }

        public async Task<EmployerRequestSummaryDto?> GetPendingRequestsByIdAsync(int id)
        {
            var request = await _db.EmployerRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == RequestStatus.Pending);

            if (request == null)
            {
                _logger.LogWarning("Pending employer request with ID {RequestId} not found.", id);
                return null;
            }

            var requestResponse = new EmployerRequestSummaryDto
            {
                Id = request.Id,
                UserId = request.UserId,
                CompanyName = request.CompanyName,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                Status = request.Status,
                Description = request.Description
            };

            return requestResponse;
        }


        public async Task<BaseResponseDto> ApproveEmployerRequestAsync(int requestId, ApproveEmployerDto dto, string adminId)
        {
            var request = await _db.EmployerRequests.FindAsync(requestId);
            if (request == null) return new BaseResponseDto
            {
                IsSuccess = false,
                ErrorType = ErrorType.NotFound,
                ResponseMessage = "Request not found"
            };
            if (request.Status != RequestStatus.Pending)
            {
                _logger.LogWarning("Employer request with ID {RequestId} is not in pending status and cannot be approved. Current status: {Status}", requestId, request.Status);
                return new BaseResponseDto
                {
                    ResponseMessage = "Request cannot be approved",
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest
                };
            }

            var user = await _db.Users.FindAsync(request.UserId);
            if (user == null)
            {
                request.Status = RequestStatus.Rejected;
                request.AdminComment = "User not found";
                request.ReviewedAt = DateTimeOffset.UtcNow;
                request.ReviewedBy = adminId;
                var responseMessage = "User not found, request is rejected";
                await _db.SaveChangesAsync();
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = responseMessage
                };
            }

            var isEmployer = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (isEmployer)
            {
                request.Status = RequestStatus.Approved;
                request.ReviewedBy = adminId;
                request.ReviewedAt = DateTimeOffset.UtcNow;
                var responseMessage = "User already has Employer role — request marked approved";
                request.AdminComment = responseMessage;
                await _db.SaveChangesAsync();
                return new BaseResponseDto
                {
                    ErrorType = ErrorType.None,
                    IsSuccess = true,
                    ResponseMessage = responseMessage,
                };
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            _logger.LogInformation("Starting transaction to approve employer request with ID {RequestId} for user {UserId}.", requestId, user.Id);

            try
            {
                var existingProfile = await _profileService.GetEmployerProfileByUserIdAsync(user.Id);
                if (existingProfile == null)
                {
                    var profile = _profileService.CreateEmployerProfile(user.Id, new EmployerRequest
                    {
                        UserId = user.Id,
                        CompanyName = request.CompanyName,
                        Website = request.Website,
                        Description = request.Description,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                    await _db.EmployerProfiles.AddAsync(profile);
                    _logger.LogInformation("Creating employer profile for user {UserId} with company name {CompanyName}", user.Id, request.CompanyName);
                }
                else
                {
                    existingProfile.UpdateFromRequest(new EmployerRequest
                    {
                        CompanyName = request.CompanyName,
                        Website = request.Website,
                        Description = request.Description,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }

                var roleResult = await _userManager.AddToRoleAsync(user, RolesName.Employer);
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var responseMessage = "Failed to assign Employer role to the user.";
                    _logger.LogError("Error assigning Employer role to user {UserId}: {Errors}", user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    return new BaseResponseDto
                    {
                        IsSuccess = false,
                        ErrorType = ErrorType.InternalServerError,
                        ResponseMessage = responseMessage,
                        Errors = roleResult.Errors.Select(e => e.Description).ToList()
                    };
                }

                request.Status = RequestStatus.Approved;
                request.ReviewedBy = adminId;
                request.ReviewedAt = DateTimeOffset.UtcNow;
                request.AdminComment = dto.AdminComment;
                request.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Successfully approved employer request with ID {RequestId} for user {UserId}. Approved by admin: {AdminId}", requestId, user.Id, adminId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while approving employer request with ID {RequestId} for user {UserId}.", requestId, user.Id);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.InternalServerError,
                    ResponseMessage = "An unexpected error occurred.",
                };
            }
            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = dto.AdminComment,
            };
        }


        public async Task<BaseResponseDto> RejectEmployerRequestAsync(int requestId, RejectEmployerDto dto, string adminId)
        {
            var request = await _db.EmployerRequests.FindAsync(requestId);
            if (request == null)
            {
                _logger.LogWarning("Employer request with ID {RequestId} not found for rejection.", requestId);
                return new BaseResponseDto
                {
                    ErrorType = ErrorType.NotFound,
                    IsSuccess = false,
                    ResponseMessage = "Request not found",
                };
            }

            if (request.Status != RequestStatus.Pending) return new BaseResponseDto
            {
                ErrorType = ErrorType.BadRequest,
                IsSuccess = false,
                ResponseMessage = "Request cannot be rejected",
            };

            request.Status = RequestStatus.Rejected;
            request.AdminComment = dto.reason;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            request.ReviewedBy = adminId;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Employer request with ID {RequestId} has been rejected by admin {AdminId}. Reason: {Reason}", requestId, adminId, dto.reason);

            return new BaseResponseDto
            {
                ErrorType = ErrorType.None,
                IsSuccess = true,
                ResponseMessage = "Request successfully rejected",
            };
        }

    }
}
