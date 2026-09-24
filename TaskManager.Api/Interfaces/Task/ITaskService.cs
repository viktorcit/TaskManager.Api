using TaskManager.Api.Data.Contracts;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.Task;
using TaskManager.Api.Data.DTO.ResponseDTOs.Tasks;
using TaskManager.Api.Entity;

namespace TaskManager.Api.Interfaces.Task
{
    public interface ITaskService
    {
        Task<List<TaskSummaryResponseDto>> GetTasksInProgressAsync();
        Task<TaskResponseDto?> GetTaskByIdAsync(int taskId);
        Task<BaseResponseDto<List<TaskSummaryResponseDto>>> GetUserCreatedTasksAsync(ApplicationUser user);
        Task<BaseResponseDto<List<TaskSummaryResponseDto>>> GetUserPerformingTasksAsync(ApplicationUser user);
        Task<BaseResponseDto<TaskResponseDto>> GetUserPerformingTaskByIdAsync(int taskId, ApplicationUser user);
        Task<BaseResponseDto<TaskResponseDto>> CreateTaskAsync(CreateTaskRequestDto dto, ApplicationUser user);
        Task<BaseResponseDto> DeleteTaskAsync(int taskId, string userId);

    }
}
