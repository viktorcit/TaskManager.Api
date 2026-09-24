using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.Contracts;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.Task;
using TaskManager.Api.Data.DTO.ResponseDTOs.Tasks;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.Task;
using TaskStatus = TaskManager.Api.Enums.TaskStatus;

namespace TaskManager.Api.Services.TaskItem
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            ILogger<TaskService> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<List<TaskSummaryResponseDto>> GetTasksInProgressAsync()
        {
            var tasks = await _db.Tasks
                .AsNoTracking()
                .Where(t => t.Status == TaskStatus.InProgress)
                .Select(t => new TaskSummaryResponseDto
                {
                    TaskId = t.Id,
                    Title = t.Title
                }).ToListAsync();

            return tasks;
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int taskId)
        {
            var task = await _db.Tasks
                .FindAsync(taskId);
            if (task == null)
                return null;

            List<UserShortInfo> performersInTask = await _db.TaskPerformers
                .Where(tp => tp.TaskId == task.Id)
                .Select(tp => new UserShortInfo
                {
                    UserId = tp.PerformerId,
                    UserName = tp.Performer.UserName,
                    Name = tp.Performer.Name
                }).ToListAsync();

            var responseTask = CreateTaskResponseDto(task, performersInTask);

            return responseTask;
        }


        public async Task<BaseResponseDto<List<TaskSummaryResponseDto>>> GetUserCreatedTasksAsync(ApplicationUser user)
        {
            var userTasks = await _db.Tasks
                .Where(t => t.OwnerId == user.Id)
                .ToListAsync();

            var responseTasks = userTasks.Select(t => new TaskSummaryResponseDto
            {
                TaskId = t.Id,
                Title = t.Title,
            }).ToList();
            _logger.LogInformation("User {UserId} retrieved their created tasks. Total tasks: {TaskCount}",
                user.Id, responseTasks.Count);

            return ResponseFactory.Ok(responseTasks);
        }


        public async Task<BaseResponseDto<List<TaskSummaryResponseDto>>> GetUserPerformingTasksAsync(ApplicationUser user)
        {
            var isEmployer = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (isEmployer)
            {
                return ResponseFactory.Fail<List<TaskSummaryResponseDto>>(ResponseType.Forbidden);
            }

            var tasks = await _db.TaskPerformers
                .Where(tp => tp.PerformerId == user.Id)
                .Select(tp => tp.Task)
                .ToListAsync();
            if (tasks.Count == 0)
            {
                return ResponseFactory.Fail<List<TaskSummaryResponseDto>>(ResponseType.NoContent);
            }

            var responseTasks = tasks.Select(t => new TaskSummaryResponseDto
            {
                TaskId = t.Id,
                Title = t.Title
            }).ToList();
            _logger.LogInformation("User {UserId} retrieved their performing tasks. Total tasks: {TaskCount}", user.Id, responseTasks.Count);

            return ResponseFactory.Ok(responseTasks);
        }


        public async Task<BaseResponseDto<TaskResponseDto>> GetUserPerformingTaskByIdAsync(int taskId, ApplicationUser user)
        {
            var isEmployer = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (isEmployer)
            {
                return ResponseFactory.Fail<TaskResponseDto>(ResponseType.Forbidden);
            }

            var task = await _db.TaskPerformers
                .Where(tp => tp.PerformerId == user.Id && tp.TaskId == taskId && tp.Task.Status == TaskStatus.InProgress)
                .Select(tp => tp.Task)
                .FirstOrDefaultAsync();
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for user {UserId}.", taskId, user.Id);
                return ResponseFactory.Fail<TaskResponseDto>(ResponseType.NotFound);
            }
            List<UserShortInfo> performersInTask = await _db.TaskPerformers
                .Where(tp => tp.TaskId == task.Id)
                .Select(tp => new UserShortInfo
                {
                    UserId = tp.PerformerId,
                    UserName = tp.Performer.UserName,
                    Name = tp.Performer.Name
                }).ToListAsync();

            var responseTask = CreateTaskResponseDto(task, performersInTask);
            _logger.LogInformation("User {UserId} retrieved performing task with ID {TaskId}.", user.Id, taskId);

            return ResponseFactory.Ok(responseTask);
        }


        public async Task<BaseResponseDto<TaskResponseDto>> CreateTaskAsync(CreateTaskRequestDto dto, ApplicationUser user)
        {
            if(dto.DueDate != null)
            {
                if (dto.DueDate < DateTime.UtcNow)
                {
                    return ResponseFactory.Fail<TaskResponseDto>(ResponseType.BadRequest, "The due date cannot be earlier than today.");
                }
            }

            var task = CreateTaskModel(dto, user);

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User {OwnerId} created a new task with ID {TaskId}.", user.Id, task.Id);

            List<UserShortInfo> performersInTask = await _db.TaskPerformers
                .Where(tp => tp.TaskId == task.Id)
                .Select(tp => new UserShortInfo
                {
                    UserId = user.Id,
                    UserName = tp.Performer.UserName,
                    Name = tp.Performer.Name
                }).ToListAsync();

            var responseTask = CreateTaskResponseDto(task, performersInTask);

            return ResponseFactory.Ok(responseTask);
        }


        public async Task<BaseResponseDto> DeleteTaskAsync(int taskId, string userId)
        {
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.OwnerId == userId);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion by user {UserId}.", taskId, userId);
                return ResponseFactory.Fail(ResponseType.NotFound);
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User {UserId} deleted task with ID {TaskId}.", userId, taskId);

            return ResponseFactory.Ok("Task deleted successfully.");
        }


        //private static methods
        private static TaskResponseDto CreateTaskResponseDto(TaskModel task, List<UserShortInfo> performersInTask)
        {
            var responseTasks = new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CanAnyoneJoin = task.CanAnyoneJoin,
                OwnerUsername = task.OwnerUserName,
                DueDate = task.DueDate,
                Status = task.Status,
                Checklist = task.Checklist.ToList(),
                Performers = performersInTask
            };
            return responseTasks;
        }

        private static TaskModel CreateTaskModel(CreateTaskRequestDto dto, ApplicationUser user)
        {
            var task = new TaskModel
            {
                OwnerUserName = user.UserName,
                OwnerId = user.Id,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                CanAnyoneJoin = dto.CanAnyoneJoin,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = Enums.TaskStatus.InProgress,
                Checklist = dto.Checklist.ToList(),
            };
            return task;
        }
    }
}

