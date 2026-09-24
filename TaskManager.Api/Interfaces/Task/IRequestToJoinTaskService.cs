using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.JoinToTask;
using TaskManager.Api.Data.DTO.ResponseDTOs.Tasks;
using TaskManager.Api.Entity;

namespace TaskManager.Api.Interfaces.Task
{
    public interface IRequestToJoinTaskService
    {
        Task<BaseResponseDto<List<RequestToJoinTaskSummaryResponseDto>>> GetJoinToTaskRequestsAsync(int taskId, string ownerId);
        Task<BaseResponseDto<RequestToJoinTaskResponseDto>> GetJoinToTaskRequestByIdAsync(int requestId, string ownerId);
        Task<BaseResponseDto> ApproveJoinToTaskRequestAsync(int requestId, string ownerId);
        Task<BaseResponseDto> RejectJoinToTaskRequestAsync(int requestId, string ownerId);
        Task<BaseResponseDto> JoinTaskAsync(int taskId, ApplicationUser user);
        Task<BaseResponseDto> CreateJoinRequestAsync(int taskId, JoinToTaskRequestDto dto, ApplicationUser user);
    }
}
