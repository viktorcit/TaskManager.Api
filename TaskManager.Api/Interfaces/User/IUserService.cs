using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.Admin;
using TaskManager.Api.Data.DTO.ResponseDTOs.Admin;

namespace TaskManager.Api.Interfaces.User
{
    public interface IUserService
    {
        Task<UserDataResponseDto?> GetUserByIdAsync(string userId);
        Task<BaseResponseDto> DeleteUserAsync(string userId, string adminId);
        Task<BaseResponseDto> UpdateUserAsync(string userId, string adminId, UpdateUserRequestDto dto);
    }
}