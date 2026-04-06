using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.EmployerDto;

namespace TaskManager.Api.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<EmployerRequestSummaryDto>> GetPendingEmployerRequestsAsync();
        Task<EmployerRequestSummaryDto?> GetPendingRequestsByIdAsync(int id);
        Task<BaseResponseDto> ApproveEmployerRequestAsync(int requestId, ApproveEmployerDto dto, string adminId);
        Task<BaseResponseDto> RejectEmployerRequestAsync(int id, RejectEmployerDto dto, string adminId);

    }
}
