using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.JoinDto;
using TaskManager.Api.Data.DTO.JoinToTaskDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Services
{
    public class RequestToJoinService : IRequestToJoinService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RequestToJoinService> _logger;


        public RequestToJoinService(AppDbContext db, UserManager<ApplicationUser> userManager, ILogger<RequestToJoinService> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<List<JoinToTaskRequestSummaryDto>> GetJoinToTaskRequestsAsync(string ownerId)
        {
            var tasks = await _db.Tasks
                .Where(t => t.OwnerId == ownerId)
                .Select(t => t.Id)
                .ToListAsync();

            var requests = await _db.JoinToTaskRequests
                .Where(r => tasks.Contains(r.TaskId))
                .Select(r => new JoinToTaskRequestSummaryDto
                {
                    Id = r.Id,
                    TaskId = r.TaskId,
                    UserId = r.UserId,
                    UserName = r.UserName,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();

            return requests;
        }


        public async Task<JoinToTaskRequestSummaryDto?> GetJoinToTaskRequestByIdAsync(int requestId, string ownerId)
        {
            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Task.OwnerId == ownerId);
            if (request == null)
                return null;

            var responseRequest = new JoinToTaskRequestSummaryDto
            {
                Id = request.Id,
                TaskId = request.TaskId,
                UserId = request.UserId,
                UserName = request.UserName,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                Description = request.Description
            };

            return responseRequest;
        }


        public async Task<BaseResponseDto> ApproveJoinToTaskRequestAsync(int requestId, string ownerId)
        {
            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Task.OwnerId == ownerId);
            if (request == null)
            {
                _logger.LogWarning("Attempt to approve non-existent request with ID {RequestId} by owner {OwnerId}", requestId, ownerId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Request not found"
                };
            }
            if (request.Status == RequestStatus.Approved)
            {
                _logger.LogInformation("Attempt to approve already approved request with ID {RequestId} by owner {OwnerId}", requestId, ownerId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Request has already been approved"
                };
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "User not found."
                };

            var userExistsInTask = await _db.Tasks
                .Where(t => t.Id == request.TaskId)
                .AnyAsync(t => t.Performers.Contains(user));
            if (userExistsInTask)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "User is already a performer of this task."
                };

            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId);
            if (task == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Task not found."
                };

            request.Status = RequestStatus.Approved;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            task.Performers.Add(user);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Request with ID {RequestId} approved by owner {OwnerId}. User {UserId} added to task {TaskId}", requestId, ownerId, user.Id, task.Id);

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Request has been approved."
            };
        }


        public async Task<BaseResponseDto> RejectJoinToTaskRequestAsync(int requestId, string ownerId)
        {
            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Task.OwnerId == ownerId);
            if (request == null)
            {
                _logger.LogWarning("Attempt to reject non-existent request with ID {RequestId} by owner {OwnerId}", requestId, ownerId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Request not found"
                };
            }

            request.Status = RequestStatus.Rejected;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Request with ID {RequestId} rejected by owner {OwnerId}", requestId, ownerId);

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Request has been rejected."
            };
        }


        public async Task<BaseResponseDto> JoinTaskAsync(int taskId, string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "User not found."
                };

            var userIsInRole = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (userIsInRole)
            {
                _logger.LogWarning("User with ID {UserId} attempted to join task {TaskId} but is an employer", userId, taskId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "Employers cannot join tasks as performers."
                };
            }

            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to join non-existent task with ID {TaskId}", userId, taskId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Task not found."
                };
            }
            if (task.CanAnyoneJoin == false)
            {
                _logger.LogInformation("User with ID {UserId} attempted to join task {TaskId} which is not open for direct joining", userId, taskId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "This task is not open for direct joining. Submit a join request."
                };
            }
            if (task.Performers.Any(p => p.Id == user.Id))
            {
                _logger.LogInformation("User with ID {UserId} attempted to join task {TaskId} but is already a performer", userId, taskId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You are already a performer of this task."
                };
            }

            task.Performers.Add(user);
            await _db.SaveChangesAsync();

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "You have successfully joined the task."
            };
        }


        public async Task<BaseResponseDto> RequestToJoinTaskAsync(int taskId, JoinToTaskRequestDto dto, string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };
            var userIsInRole = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (userIsInRole)
            {
                _logger.LogWarning("User with ID {UserId} attempted to request to join task {TaskId} but is an employer", userId, taskId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "Employers cannot join tasks as performers."
                };
            }
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Task not found."
                };
            }
            if (task.CanAnyoneJoin == true)
            {
                _logger.LogInformation("User with ID {UserId} attempted to request to join task {TaskId} which is open for direct joining", userId, taskId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "This task is open for joining. Just joining."
                };
            }
            if (task.Performers.Any(p => p.Id == user.Id))
            {
                _logger.LogInformation("User with ID {UserId} attempted to request to join task {TaskId} but is already a performer", userId, taskId);
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You are already a performer of this task."
                };
            }
            var username = user.UserName;
            if (username == null)
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Username not found."
                };
            }

            var joinRequest = new JoinToTaskRequest
            {
                TaskId = task.Id,
                UserId = user.Id,
                UserName = username,
                Task = task,
                Status = RequestStatus.Pending,
                Description = dto.Description
            };

            _db.JoinToTaskRequests.Add(joinRequest);
            await _db.SaveChangesAsync();
            _logger.LogInformation("User with ID {UserId} submitted a request to join task {TaskId}", userId, taskId);

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Your request to join the task has been sent to the owner."
            };
        }
    }
}
