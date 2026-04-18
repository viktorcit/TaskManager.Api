using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Model;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Enums;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TaskService> _logger;


        public TaskService(AppDbContext db, UserManager<ApplicationUser> userManager, ILogger<TaskService> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<List<TaskItemSummaryDto>> GetTasksInProgressAsync()
        {
            var tasks = await _db.Tasks
                .Where(t => t.Status == Enums.TaskStatus.InProgress)
                .ToListAsync();


            var responseTasks = tasks.Select(t => new TaskItemSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                OwnerUsername = t.OwnerUsername,
            }).ToList();

            return responseTasks;
        }


        public async Task<List<TaskItemSummaryDto>> GetAllTasksAsync()
        {
            var tasks = await _db.Tasks.ToListAsync();

            var responseTasks = tasks.Select(t => new TaskItemSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                OwnerUsername = t.OwnerUsername,
                Status = t.Status
            }).ToList();

            return responseTasks;
        }


        public async Task<TaskItemDto?> GetTaskByIdAsync(int taskId)
        {
            var task = await _db.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return null;

            var userPerformerTasks = task.Performers
                .Select(p => new UserShortDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Username = p.UserName
                }).ToList();

            var responseTasks = new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                OwnerUsername = task.OwnerUsername,
                CreatedAt = task.CreatedAt,
                Description = task.Description,
                CanAnyoneJoin = task.CanAnyoneJoin,
                DueDate = task.DueDate,
                Status = task.Status,
                Performers = userPerformerTasks.ToList(),
                Checklist = task.Checklist.ToList(),
            };

            return responseTasks;
        }


        public async Task<BaseResponseWithDataDto<List<TaskItemSummaryDto>>> GetUserCreatedTasksAsync(string userId)
        {
            var user = await GetCurrentUserByIdAsync(userId);
            if (user == null)
                return new BaseResponseWithDataDto<List<TaskItemSummaryDto>>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };
            var userTasks = await _db.Tasks
                .Where(t => t.OwnerId == user.Id)
                .ToListAsync();

            var responseTasks = userTasks.Select(t => new TaskItemSummaryDto
            {
                Id = t.Id,
                OwnerUsername = t.OwnerUsername,
                Status = t.Status
            }).ToList();
            _logger.LogInformation("User {UserId} retrieved their created tasks. Total tasks: {TaskCount}", userId, responseTasks.Count);

            return new BaseResponseWithDataDto<List<TaskItemSummaryDto>>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "",
                Data = responseTasks
            };
        }


        public async Task<BaseResponseWithDataDto<List<TaskItemSummaryDto>>> GetUserPerformingTasksAsync(string userId)
        {
            var performer = await GetCurrentUserByIdAsync(userId);
            if (performer == null)
            {
                return new BaseResponseWithDataDto<List<TaskItemSummaryDto>>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };
            }
            var isInRole = await _userManager.IsInRoleAsync(performer, RolesName.Employer);
            if (isInRole)
            {
                _logger.LogWarning("User {UserId} with role 'Employer' attempted to access performing tasks.", userId);
                return new BaseResponseWithDataDto<List<TaskItemSummaryDto>>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "Only a regular user can access their tasks."
                };
            }

            var tasks = await _db.Tasks.Where(t => t.Performers.Contains(performer)).ToListAsync();

            var responseTasks = tasks.Select(t => new TaskItemSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                OwnerUsername = t.OwnerUsername,
                OwnerId = t.OwnerId,
                CreatedAt = t.CreatedAt
            }).ToList();
            _logger.LogInformation("User {UserId} retrieved their performing tasks. Total tasks: {TaskCount}", userId, responseTasks.Count);

            return new BaseResponseWithDataDto<List<TaskItemSummaryDto>>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "",
                Data = responseTasks
            };
        }


        public async Task<BaseResponseWithDataDto<TaskItemDto>> GetUserPerformingTaskByIdAsync(int taskId, string userId)
        {
            var performer = await GetCurrentUserByIdAsync(userId);
            if (performer == null)
            {
                return new BaseResponseWithDataDto<TaskItemDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };
            }
            var isInRole = await _userManager.IsInRoleAsync(performer, RolesName.Employer);
            if (isInRole)
            {
                _logger.LogWarning("User {UserId} with role 'Employer' attempted to access performing task with ID {TaskId}.", userId, taskId);
                return new BaseResponseWithDataDto<TaskItemDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "Only a regular user can access their tasks."
                };
            }
            var task = await _db.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Performers.Any(p => p.Id == performer.Id));
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for user {UserId}.", taskId, userId);
                return new BaseResponseWithDataDto<TaskItemDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "The task with this ID that you are performing was not found."
                };
            }

            var responseTask = new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                OwnerUsername = task.OwnerUsername,
                OwnerId = task.OwnerId,
                CreatedAt = task.CreatedAt
            };
            _logger.LogInformation("User {UserId} retrieved performing task with ID {TaskId}.", userId, taskId);

            return new BaseResponseWithDataDto<TaskItemDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "",
                Data = responseTask
            };
        }


        public async Task<BaseResponseWithDataDto<TaskItemResponseDto>> CreateTaskAsync(CreateTaskDto dto, string userId)
        {
            var owner = await GetCurrentUserByIdAsync(userId);
            if (owner == null)
            {
                return new BaseResponseWithDataDto<TaskItemResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };
            }

            var performers = await _userManager.Users
                .Where(u => dto.PerformersId.Contains(u.Id))
                .ToListAsync();
            if (performers.Count != dto.PerformersId.Count)
            {
                return new BaseResponseWithDataDto<TaskItemResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "One or more performers not found."
                };
            }

            var ownerUsername = owner.UserName;
            if (ownerUsername == null)
            {
                return new BaseResponseWithDataDto<TaskItemResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Owner username not found."
                };
            }

            var task = new TaskItem
            {
                Owner = owner,
                OwnerId = owner.Id,
                OwnerUsername = ownerUsername,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                CanAnyoneJoin = dto.CanAnyoneJoin,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = Enums.TaskStatus.InProgress,
                Performers = performers.ToList(),
                Checklist = dto.Checklist.ToList(),
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User {UserId} created a new task with ID {TaskId}.", userId, task.Id);

            var performersInTask = task.Performers.Select(p => new UserShortDto
            {
                Id = p.Id,
                Name = p.Name,
                Username = p.UserName,
            }).ToList();

            var responseTask = new TaskItemResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                Status = task.Status,
                OwnerId = task.OwnerId,
                OwnerUsername = task.OwnerUsername,
                Performers = performersInTask,
                Checklist = task.Checklist.ToList(),
            };

            return new BaseResponseWithDataDto<TaskItemResponseDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "",
                Data = responseTask
            };
        }


        public async Task<BaseResponseDto> DeleteTaskAsync(int taskId, string userId)
        {
            var user = await GetCurrentUserByIdAsync(userId);
            if (user == null)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };
            }
            var task = await _db.Tasks.FindAsync(taskId);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion by user {UserId}.", taskId, userId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Task not found"
                };
            }
            if (task.OwnerId != user.Id)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "You can`t delete not yours task."
                };
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User {UserId} deleted task with ID {TaskId}.", userId, taskId);

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Task deleted successfully"
            };
        }

        
        //private methods
        private async Task<ApplicationUser?> GetCurrentUserByIdAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user;
        }


    }
}

