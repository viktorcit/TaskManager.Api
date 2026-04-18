using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.ProfileDto;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            AppDbContext db,
            ILogger<ProfileService> logger)
        {
            _userManager = userManager;
            _db = db;
            _logger = logger;
        }




        public async Task<List<PublicProfileDto>> GetAllProfilesAsync(int page = 1, int pageSize = 20)
        {
            var profiles = await _userManager.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = profiles.Select(u => new PublicProfileDto
            {
                Id = u.Id,
                Name = u.Name,
                Nickname = u.Nickname
            }).ToList();

            return response;
        }

        public async Task<PublicProfileDto?> GetProfileOfUserAsync(string nickname)
        {
            var findProfile = await _userManager.Users.FirstOrDefaultAsync(u => u.Nickname.Normalize() == nickname.Normalize());
            if (findProfile == null)
            {
                _logger.LogWarning("Profile with nickname {Nickname} not found", nickname);
                return null;
            }

            var responseProfile = new PublicProfileDto
            {
                Id = findProfile.Id,
                Nickname = findProfile.Nickname,
                Name = findProfile.Name,
            };
            return responseProfile;
        }

        public async Task<PrivateProfileDto?> GetProfileAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            var userPerformerTasks = await _db.Tasks
                .Where(t => t.Performers.Any(p => p.Id == userId))
                .Select(t => new TaskItemShortDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status
                }).ToListAsync();
            var userOwnerTasks = await _db.Tasks
                .Where(t => t.Owner.Id == userId)
                .Select(t => new TaskItemShortDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status
                }).ToListAsync();


            var response = new PrivateProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                Age = user.Age,
                CreatedAt = user.CreatedAt,
                PerformerTasks = userPerformerTasks,
                OwnerTasks = userOwnerTasks
            };

            return response;
        }


        public async Task<BaseResponseDto> DeleteProfileAsync(string userId)
        {
            var userProfile = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userProfile == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };

            var isInRole = await _userManager.IsInRoleAsync(userProfile, RolesName.Admin);
            if (isInRole)
            {
                _logger.LogWarning("Attempt to delete admin account with id {UserId}", userId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "You cant delete admin account"
                };
            }

            await _userManager.DeleteAsync(userProfile);
            _logger.LogInformation("User with id {UserId} deleted their profile", userId);

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = ""
            };
        }


        public async Task<BaseResponseDto> UpdateProfileAsync(UpdateProfileDto dto, string userId)
        {
            var profile = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (profile == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };

            var isInRole = await _userManager.IsInRoleAsync(profile, RolesName.Admin);
            if (isInRole)
            {
                _logger.LogWarning("Attempt to update admin account with id {UserId}", userId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "You cant update admin account."
                };
            }

            if (dto.Name == null && dto.Age == null)
            {
                _logger.LogInformation("User with id {UserId} attempted to update profile without providing any data", userId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Nothing to update"
                };
            }

            if (dto.Name != null)
                profile.Name = dto.Name;
            if (dto.Age != null)
                profile.Age = dto.Age;

            await _userManager.UpdateAsync(profile);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User with id {UserId} updated their profile", userId);

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Profile updated successfully"
            };
        }


    }
}
