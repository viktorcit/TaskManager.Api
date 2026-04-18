using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.UserDto;

namespace TaskManager.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<AdminUserDto>> GetUsers();
        Task<AdminUserDto?> GetUserByIdAsync(string userId);
        Task<BaseResponseDto> DeleteUserAsync(string userId, string adminId);
        Task<BaseResponseDto> UpdateUserAsync(string userId, string adminId, AdminUpdateUserDto dto);
    }
}