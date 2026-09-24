using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.Contracts;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.Admin;
using TaskManager.Api.Data.DTO.ResponseDTOs.Admin;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.User;
using TaskStatus = TaskManager.Api.Enums.TaskStatus;

namespace TaskManager.Api.Services.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            ILogger<UserService> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<UserDataResponseDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

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

            var response = new UserDataResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Nickname = user.UserName,
                Age = user.Age,
                CreatedAt = user.CreatedAt,
                PerformerTasks = userPerformerTasks,
                OwnerTasks = userOwnerTasks,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd
            };

            return response;
        }


        public async Task<BaseResponseDto> DeleteUserAsync(string userId, string adminId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found for deletion", userId);
                return ResponseFactory.Fail(ResponseType.NotFound);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, RolesName.Admin);
            if (isAdmin)
            {
                _logger.LogWarning("Admin with id {AdminId} attempted to delete admin account with id {UserId}", adminId, userId);
                return ResponseFactory.Fail(ResponseType.Forbidden);
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User with id {UserId} was deleted by admin with id {AdminId}", userId, adminId);

            return ResponseFactory.Ok($"User with id {userId} was deleted");
        }


        public async Task<BaseResponseDto> UpdateUserAsync(string userId, string adminId, UpdateUserRequestDto dto)
        {
            if (dto.GetType().GetProperties().All(p => p.GetValue(dto) is null))
            {
                return ResponseFactory.Fail(ResponseType.BadRequest);
            }

            if (dto.UserName != null)
            {
                var userNameExist = await _userManager.FindByNameAsync(dto.UserName);
                if (userNameExist != null)
                {
                    return ResponseFactory.Fail(ResponseType.BadRequest, "A userName like that already exists.");
                }
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with id {UserId} not found for update", userId);
                return ResponseFactory.Fail(ResponseType.NotFound);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, RolesName.Admin);
            if (isAdmin)
            {
                _logger.LogWarning("Attempt to update admin account with id {UserId} was blocked", userId);
                return ResponseFactory.Fail(ResponseType.Forbidden);
            }

            if (dto.Name != null)
                user.Name = dto.Name;
            if (dto.Age != null)
                user.Age = dto.Age;
            if (dto.UserName != null)
            {
                await ChangeUsernameInRequestAndTasksAsync(user.UserName, dto.UserName);
                var normalizeUserName = _userManager.NormalizeName(dto.UserName);
                user.NormalizedUserName = normalizeUserName;
                user.UserName = dto.UserName;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("User with id {UserId} was updated by admin with id {AdminId}", userId, adminId);

            return ResponseFactory.Ok($"User with id {userId} was updated");
        }



        //private methods
        private async Task ChangeUsernameInRequestAndTasksAsync(string oldUserName, string newUserName)
        {
            var userOwnerTasks = await _db.Tasks
                .Where(t => t.OwnerUserName == oldUserName)
                .ToListAsync();
            if (userOwnerTasks.Count != 0)
            {
                foreach (var task in userOwnerTasks)
                {
                    task.OwnerUserName = newUserName;
                }
            }

            var userJoinRequest = await _db.JoinToTaskRequests
                .Where(t => t.UserName == oldUserName)
                .ToListAsync();
            if (userJoinRequest.Count != 0)
            {
                foreach (var request in userJoinRequest)
                {
                    request.UserName = newUserName;
                }
            }
        }
    }
}

