using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.Contracts;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.Profile;
using TaskManager.Api.Data.DTO.ResponseDTOs.Profile;
using TaskManager.Api.Data.DTO.ResponseDTOs.Tasks;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.User;
using TaskStatus = TaskManager.Api.Enums.TaskStatus;

namespace TaskManager.Api.Services.User
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


        public async Task<PublicProfileResponseDto?> GetProfileByUsernameAsync(string userName)
        {
            var findProfile = await _userManager.FindByNameAsync(userName);
            if (findProfile == null)
            {
                _logger.LogWarning("Profile with nickname {Nickname} not found", userName);
                return null;
            }

            var responseProfile = new PublicProfileResponseDto
            {
                Nickname = findProfile.UserName,
                Name = findProfile.Name,
                Age = findProfile.Age,
            };
            return responseProfile;
        }


        public async Task<PrivateProfileResponseDto?> GetProfileAsync(ApplicationUser user)
        {
            var userPerformerTasks = await _db.TaskPerformers
                .Where(tp => tp.Performer.Id == user.Id && tp.Task.Status == TaskStatus.InProgress)
                .Select(tp => new TaskModelShortInfo
                {
                    TaskId = tp.Task.Id,
                    Title = tp.Task.Title,
                    Status = tp.Task.Status
                }).ToListAsync();
            var userOwnerTasks = await _db.Tasks
                .Where(t => t.OwnerId == user.Id && t.Status == TaskStatus.InProgress)
                .Select(t => new TaskModelShortInfo
                {
                    TaskId = t.Id,
                    Title = t.Title,
                    Status = t.Status
                }).ToListAsync();


            var response = new PrivateProfileResponseDto
            {
                Name = user.Name,
                Nickname = user.UserName,
                Age = user.Age,
                PerformerTasks = userPerformerTasks,
                OwnerTasks = userOwnerTasks
            };
            return response;
        }


        public async Task<BaseResponseDto> UpdateProfileAsync(UpdateProfileRequestDto dto, ApplicationUser user)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, RolesName.Admin);
            if (isAdmin)
            {
                return ResponseFactory.Fail(ResponseType.Forbidden);
            }

            if (dto.Name == null && dto.Age == null)
            {
                _logger.LogInformation("User with id {UserId} attempted to update profile without providing any data", user.Id);
                return ResponseFactory.Fail(ResponseType.BadRequest, "Nothing to update");
            }

            if (dto.Name != null)
                user.Name = dto.Name;
            if (dto.Age != null)
                user.Age = dto.Age;

            await _userManager.UpdateAsync(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User with id {UserId} updated their profile", user.Id);

            return ResponseFactory.Ok("Profile updated successfully");
        }


        public async Task<BaseResponseDto> DeleteProfileAsync(ApplicationUser user)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user, RolesName.Admin);
            if (isAdmin)
            {
                return ResponseFactory.Fail(ResponseType.Forbidden);
            }

            await _userManager.DeleteAsync(user);
            _logger.LogInformation("User with id {UserId} deleted their profile", user.Id);

            return ResponseFactory.Ok("Profile deleted successfully");
        }
    }
}
