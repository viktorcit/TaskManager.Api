using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.Auth;

namespace TaskManager.Api.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<BaseResponseDto<string>> RegisterAsync(RegisterRequestDto dto);
        Task<BaseResponseDto<string>> LoginAsync(LoginRequestDto dto);
    }
}
