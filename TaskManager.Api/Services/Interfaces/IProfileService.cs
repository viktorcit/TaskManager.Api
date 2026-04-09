using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.ProfileDto;

namespace TaskManager.Api.Services.Interfaces
{
    public interface IProfileService
    {
        Task<List<PublicProfileDto>> GetAllProfilesAsync(int page, int pageSize);
        Task<PublicProfileDto?> GetProfileOfUserAsync(string nickname);
        Task<PrivateProfileDto?> GetProfileAsync(string userId);
        Task<BaseResponseDto> DeleteProfileAsync(string userId);
        Task<BaseResponseDto> UpdateProfileAsync(UpdateProfileDto dto, string userId);
    }
}
