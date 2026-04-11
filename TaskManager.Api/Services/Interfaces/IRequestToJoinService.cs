using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.JoinDto;
using TaskManager.Api.Data.DTO.JoinToTaskDto;

namespace TaskManager.Api.Services.Interfaces
{
    public interface IRequestToJoinService
    {
        Task<List<JoinToTaskRequestSummaryDto>> GetJoinToTaskRequestsAsync(string ownerId);
        Task<JoinToTaskRequestSummaryDto?> GetJoinToTaskRequestByIdAsync(int id, string ownerId);
        Task<BaseResponseDto> ApproveJoinToTaskRequestAsync(int id, string ownerId);
        Task<BaseResponseDto> RejectJoinToTaskRequestAsync(int id, string ownerId);
        Task<BaseResponseDto> JoinTaskAsync(int taskId, string userId);
        Task<BaseResponseDto> RequestToJoinTaskAsync(int taskId, JoinToTaskRequestDto dto, string userId);
    }
}
