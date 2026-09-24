using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.Profile;
using TaskManager.Api.Data.DTO.ResponseDTOs.Profile;
using TaskManager.Api.Entity;

namespace TaskManager.Api.Interfaces.User
{
    public interface IProfileService
    {
        Task<PublicProfileResponseDto?> GetProfileByUsernameAsync(string UserName);
        Task<PrivateProfileResponseDto?> GetProfileAsync(ApplicationUser user);
        Task<BaseResponseDto> DeleteProfileAsync(ApplicationUser user);
        Task<BaseResponseDto> UpdateProfileAsync(UpdateProfileRequestDto dto, ApplicationUser user);
    }
}
