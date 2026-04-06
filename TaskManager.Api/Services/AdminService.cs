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
        private readonly EmployerProfileService _profileService;

        public AdminService(AppDbContext db, UserManager<ApplicationUser> userManager, EmployerProfileService profileService)
        {
            _db = db;
            _userManager = userManager;
            _profileService = profileService;
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
            if (request.Status != RequestStatus.Pending) return new BaseResponseDto
            {
                ResponseMessage = "Request cannot be approved",
                IsSuccess = false,
                ErrorType = ErrorType.BadRequest
            };

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

            try
            {
                var existingProfile = await _profileService.GetProfileByUserIdAsync(user.Id);
                if (existingProfile == null)
                {
                    var profile = _profileService.CreateProfile(user.Id, new EmployerRequest
                    {
                        UserId = user.Id,
                        CompanyName = request.CompanyName,
                        Website = request.Website,
                        Description = request.Description,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                    await _db.EmployerProfiles.AddAsync(profile);
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
                    return new BaseResponseDto
                    {
                        IsSuccess = false,
                        ErrorType = ErrorType.InternalServerError,
                        ResponseMessage = responseMessage
                    };
                }

                request.Status = RequestStatus.Approved;
                request.ReviewedBy = adminId;
                request.ReviewedAt = DateTimeOffset.UtcNow;
                request.AdminComment = dto.AdminComment;
                request.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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


        public async Task<BaseResponseDto> RejectEmployerRequestAsync(int id, RejectEmployerDto dto, string adminId)
        {
            var request = await _db.EmployerRequests.FindAsync(id);
            if (request == null) return new BaseResponseDto
            {
                ErrorType = ErrorType.NotFound,
                IsSuccess = false,
                ResponseMessage = "Request not found",
            };

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

            return new BaseResponseDto
            {
                ErrorType = ErrorType.None,
                IsSuccess = true,
                ResponseMessage = "Request successfully rejected",
            };
        }

    }
}
