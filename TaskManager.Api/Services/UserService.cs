using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }



        public async Task<List<AdminUserDto>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var response = users.Select(u => new AdminUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Nickname = u.Nickname,
                Age = u.Age,
                CreatedAt = u.CreatedAt,
            }).ToList();

            return response;
        }


        public async Task<AdminUserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var userPerformerTasks = await _db.Tasks.Where(t => t.Performers.Any(p => p.Id == user.Id))
                .Select(p => new TaskItemShortDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Status = p.Status,
                }).ToListAsync();
            var userOwnerTasks = await _db.Tasks.Where(t => t.Owner.Id == user.Id)
                .Select(p => new TaskItemShortDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Status = p.Status,
                }).ToListAsync();

            var response = new AdminUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                Age = user.Age,
                CreatedAt = user.CreatedAt,
                PerformerTasks = userPerformerTasks,
                OwnerTasks = userOwnerTasks,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd
            };

            return response;
        }


        public async Task<BaseResponseDto> DeleteUserAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) 
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = $"User with id {userId} not found"
                };

            var isInRole = await _userManager.IsInRoleAsync(user, RolesName.Admin);
            if (isInRole)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = $"You can't delete user with id {userId} because it's admin"
                };
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "",
            };
        }


        public async Task<BaseResponseDto> UpdateUserAsync(string userId, AdminUpdateUserDto dto)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = $"User with id {userId} not found"
                };

            var isInRole = await _userManager.IsInRoleAsync(user, RolesName.Admin);
            if (isInRole)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = $"You can't update data of user with id {userId} because it's admin"
                };
            }

            if (dto.Name != null)
                user.Name = dto.Name;
            if (dto.Age != null)
                user.Age = dto.Age;
            if (dto.Nickname != null)
                user.Nickname = dto.Nickname;

            await _db.SaveChangesAsync();
            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = $"User with id {userId} was updated"
            };
        }
    }
}

