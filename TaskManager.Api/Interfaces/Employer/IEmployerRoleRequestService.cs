using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDto.Employer;
using TaskManager.Api.Data.DTO.RequestDto.EmployerDto;
using TaskManager.Api.Data.DTO.RequestDTOs.Employer;
using TaskManager.Api.Data.DTO.ResponseDTOs.Employer;
using TaskManager.Api.Entity;

namespace TaskManager.Api.Interfaces.Employer
{
    public interface IEmployerRoleRequestService
    {
        Task<BaseResponseDto<List<RequestToEmployerRoleDto>>> GetPendingEmployerRequestsAsync();
        Task<BaseResponseDto<RequestToEmployerRoleDto>> GetPendingRequestsByIdAsync(int requestId);
        Task<BaseResponseDto> ApproveEmployerRequestAsync(int requestId, ApproveEmployerRoleRequestDto dto, string adminId);
        Task<BaseResponseDto> RejectEmployerRequestAsync(int requestId, RejectEmployerRoleRequestDto dto, string adminId);
        Task<BaseResponseDto> CreateEmployerRoleRequestAsync(CreateEmployerRoleRequestDto dto, ApplicationUser user);
    }
}
