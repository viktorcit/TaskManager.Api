using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManager.Api.Data;
using TaskManager.Api.Data.Contracts;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.JoinToTask;
using TaskManager.Api.Data.DTO.ResponseDTOs.Tasks;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.Task;

namespace TaskManager.Api.Services.TaskItem
{
    public class RequestToJoinTaskService : IRequestToJoinTaskService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestToJoinTaskService> _logger;

        public RequestToJoinTaskService(AppDbContext db,
            UserManager<ApplicationUser> userManager,
            ILogger<RequestToJoinTaskService> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<BaseResponseDto<List<RequestToJoinTaskSummaryResponseDto>>> GetJoinToTaskRequestsAsync(int taskId ,string userId)
        {
            var task = await _db.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == taskId && t.OwnerId == userId);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for owner {OwnerId}", taskId, userId);
                return ResponseFactory.Fail<List<RequestToJoinTaskSummaryResponseDto>>(ResponseType.NotFound);
            }

            var requests = await _db.JoinToTaskRequests
                .AsNoTracking()
                .Where(r => r.TaskId == task.Id && r.Status == RequestStatus.Pending)
                .Select(r => new RequestToJoinTaskSummaryResponseDto
                {
                    RequestId = r.Id,
                    UserName = r.UserName
                }).ToListAsync();

            return ResponseFactory.Ok(requests);
        }


        public async Task<BaseResponseDto<RequestToJoinTaskResponseDto>> GetJoinToTaskRequestByIdAsync(int requestId, string userId)
        {
            var request = await FindRequestForTaskOwnerAsync(requestId, userId);
            if (request == null)
            {
                return ResponseFactory.Fail<RequestToJoinTaskResponseDto>(ResponseType.NotFound);
            }

            var responseRequest = new RequestToJoinTaskResponseDto
            {
                RequestId = requestId,
                UserId = request.UserId,
                UserName = request.UserName,
                Description = request.Description
            };

            return ResponseFactory.Ok(responseRequest);
        }

        public async Task<BaseResponseDto> ApproveJoinToTaskRequestAsync(int requestId, string ownerId)
        {
            var request = await _db.JoinToTaskRequests
                .FindAsync(requestId);
            if (request == null)
            {
                _logger.LogWarning("Attempt to approve non-existent request with ID {RequestId} by owner {OwnerId}", requestId, ownerId);
                return ResponseFactory.Fail(ResponseType.NotFound);
            }
            if (request.Status != RequestStatus.Pending)
            {
                _logger.LogInformation("Attempt to APPROVE a request with identifier {RequestId}" +
                    " by owner {OwnerId} that is not in the 'Pending' status.", requestId, ownerId);
                return ResponseFactory.Fail(ResponseType.Conflict, "The request does not have a 'pending' status.");
            }

            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId && t.OwnerId == ownerId);
            if (task == null)
            {
                return ResponseFactory.Fail(ResponseType.NotFound);
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                request.Status = RequestStatus.Rejected;
                await _db.SaveChangesAsync();
                return ResponseFactory.Fail(ResponseType.NotFound);
            }

            var responseMessage = await UserIsEmployerOrExistsInTasksAsync(user, request);
            if (responseMessage != null)
            {
                return ResponseFactory.Fail(ResponseType.Conflict, responseMessage);
            }

            var taskPerformer = CreateTaskPerformerEntity(task, user);

            request.Status = RequestStatus.Approved;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            _db.TaskPerformers.Add(taskPerformer);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Request with ID {RequestId} approved by owner {OwnerId}. User {UserId} added to task {TaskId}",
                requestId, ownerId, user.Id, task.Id);

            return ResponseFactory.Ok("Request has been approved and user has been added to the task.");
        }


        public async Task<BaseResponseDto> RejectJoinToTaskRequestAsync(int requestId, string ownerId)
        {
            var request = await FindRequestForTaskOwnerAsync(requestId, ownerId);
            if (request == null)
            {
                return ResponseFactory.Fail(ResponseType.NotFound);
            }
            if (request.Status != RequestStatus.Pending)
            {
                _logger.LogInformation("Attempt to REJECT a request with identifier {RequestId}" +
                    " by owner {OwnerId} that is not in the 'Pending' status.", requestId, ownerId);
                return ResponseFactory.Fail(ResponseType.Conflict, "The request does not have a 'pending' status.");
            }

            request.Status = RequestStatus.Rejected;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Request with ID {RequestId} rejected by owner {OwnerId}", requestId, ownerId);

            return ResponseFactory.Ok("Request has been rejected.");
        }


        public async Task<BaseResponseDto> JoinTaskAsync(int taskId, ApplicationUser user)
        {
            var isEmployer = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (isEmployer)
            {
                return ResponseFactory.Fail(ResponseType.Forbidden);
            }

            var task = await _db.Tasks.FindAsync(taskId);
            if (task == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to join non-existent task with ID {TaskId}", user.Id, taskId);
                return ResponseFactory.Fail(ResponseType.NotFound);
            }
            if (task.CanAnyoneJoin == false)
            {
                _logger.LogInformation("User with ID {UserId} attempted to join task {TaskId} which is not open for direct joining", user.Id, taskId);
                return ResponseFactory.Fail(ResponseType.Conflict);
            }

            var taskPerformers = await _db.TaskPerformers
                .AnyAsync(tp => tp.TaskId == taskId && tp.PerformerId == user.Id);
            if (taskPerformers)
            {
                _logger.LogInformation("User with ID {UserId} attempted to join task {TaskId} but is already a performer", user.Id, taskId);
                return ResponseFactory.Fail(ResponseType.Conflict);
            }

            var newTaskPerformer = CreateTaskPerformerEntity(task, user);

            await _db.TaskPerformers.AddAsync(newTaskPerformer);
            await _db.SaveChangesAsync();

            return ResponseFactory.Ok("You have successfully joined the task.");
        }


        public async Task<BaseResponseDto> CreateJoinRequestAsync(int taskId, JoinToTaskRequestDto dto, ApplicationUser user)
        {
            var isEmployer = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (isEmployer)
            {
                return ResponseFactory.Fail(ResponseType.Forbidden);
            }

            var joinRequestExist = await _db.JoinToTaskRequests.AnyAsync(r => r.TaskId == taskId && r.UserId == user.Id);
            if (joinRequestExist)
            {
                return ResponseFactory.Fail(ResponseType.Conflict);
            }
            var task = await _db.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return ResponseFactory.Fail(ResponseType.NotFound);
            }
            if (task.CanAnyoneJoin == true)
            {
                _logger.LogInformation("User with ID {UserId} attempted to request to join task {TaskId} which is open for direct joining", user.Id, taskId);
                return ResponseFactory.Fail(ResponseType.Conflict);
            }

            var taskPerformers = await _db.TaskPerformers
                .AnyAsync(tp => tp.TaskId == taskId && tp.PerformerId == user.Id);
            if (taskPerformers)
            {
                _logger.LogInformation("User with ID {UserId} attempted to request to join task {TaskId} but is already a performer", user.Id, taskId);
                return ResponseFactory.Fail(ResponseType.Conflict);
            }

            var joinRequest = new JoinToTaskRequest
            {
                TaskId = task.Id,
                UserId = user.Id,
                UserName = user.UserName,
                Status = RequestStatus.Pending,
                Description = dto.Description
            };

            _db.JoinToTaskRequests.Add(joinRequest);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User with ID {UserId} submitted a request to join task {TaskId}", user.Id, taskId);

            return ResponseFactory.Ok("Your request to join the task has been submitted and is pending approval.");
        }


        //private methods
        private async Task<JoinToTaskRequest?> FindRequestForTaskOwnerAsync(int requestId, string userId)
        {
            var request = await _db.JoinToTaskRequests.FindAsync(requestId);
            if (request == null) return null;

            var task = await _db.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.OwnerId == userId);
            if (task == null) return null;

            return request;
        }

        private async Task<string?> UserIsEmployerOrExistsInTasksAsync(ApplicationUser user, JoinToTaskRequest request)
        {
            var isEmployer = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (isEmployer)
            {
                request.Status = RequestStatus.Rejected;
                await _db.SaveChangesAsync();
                return "The user has an employer role so he can`t be performer";
            }

            var userExistsInTask = await _db.TaskPerformers
                .AsNoTracking()
                .Where(t => t.TaskId == request.TaskId)
                .AnyAsync(t => t.PerformerId == user.Id);
            if (userExistsInTask)
            {
                request.Status = RequestStatus.Rejected;
                await _db.SaveChangesAsync();
                return "User is already a performer of this task.";
            }
            return null;
        }

        //private static methods
        private static TaskPerformer CreateTaskPerformerEntity(TaskModel task, ApplicationUser user)
        {
            var taskPerformer = new TaskPerformer
            {
                TaskId = task.Id,
                Task = task,
                PerformerId = user.Id,
                Performer = user
            };
            return taskPerformer;
        }
    }
}
