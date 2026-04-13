using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.TasksDto;

namespace TaskManager.Api.Services.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskItemSummaryDto>> GetTasksInProgressAsync();
        Task<List<TaskItemSummaryDto>> GetAllTasksAsync();
        Task<TaskItemDto?> GetTaskByIdAsync(int id);
        Task<BaseResponseWithDataDto<List<TaskItemSummaryDto>>> GetUserCreatedTasksAsync(string userId);
        Task<BaseResponseWithDataDto<List<TaskItemSummaryDto>>> GetUserPerformingTasksAsync(string userId);
        Task<BaseResponseWithDataDto<TaskItemDto>> GetUserPerformingTaskByIdAsync(int taskId, string userId);
        Task<BaseResponseWithDataDto<TaskItemResponseDto>> CreateTaskAsync(CreateTaskDto dto, string userId);
        Task<BaseResponseDto> DeleteTaskAsync(int id, string userId);

    }
}
