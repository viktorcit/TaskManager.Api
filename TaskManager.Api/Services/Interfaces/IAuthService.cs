using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Data.DTO.UserDto;

namespace TaskManager.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponseWithDataDto<AuthResponseDto>> RegisterAsync(RegisterUserDto dto);
        Task<BaseResponseWithDataDto<AuthResponseDto>> LoginAsync(LoginUserDto dto);
        Task<BaseResponseDto> RequestEmployerAsync(RequestEmployerDto dto, string userId);
    }
}
