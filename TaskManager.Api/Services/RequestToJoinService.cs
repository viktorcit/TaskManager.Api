using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.JoinDto;
using TaskManager.Api.Data.DTO.JoinToTaskDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;
using static TaskManager.Api.Model.JoinToTaskRequest;

namespace TaskManager.Api.Controllers
{
    public class RequestToJoinService : IRequestToJoinService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;


        public RequestToJoinService(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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


        public async Task<JoinToTaskRequestSummaryDto?> GetJoinToTaskRequestByIdAsync(int id, string ownerId)
        {
            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.Task.OwnerId == ownerId);
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


        public async Task<BaseResponseDto> ApproveJoinToTaskRequestAsync(int id, string ownerId)
        {
            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.Task.OwnerId == ownerId);
            if (request == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Request not found"
                };
            if (request.Status == JoinRequestStatus.Approved)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Request has been Approved."
                };

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

            request.Status = JoinRequestStatus.Approved;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            task.Performers.Add(user);
            await _db.SaveChangesAsync();

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Request has been approved."
            };
        }


        public async Task<BaseResponseDto> RejectJoinToTaskRequestAsync(int id, string ownerId)
        {
            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.Task.OwnerId == ownerId);
            if (request == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Request not found"
                };

            request.Status = JoinToTaskRequest.JoinRequestStatus.Rejected;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();

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
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "Employers cannot join tasks as performers."
                };

            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.NotFound,
                    ResponseMessage = "Task not found."
                };
            if (task.CanAnyoneJoin == false)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "This task is not open for direct joining. Submit a join request."
                };
            if (task.Performers.Any(p => p.Id == user.Id))
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You are already a performer of this task."
                };

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
            {
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };
            }
            var userIsInRole = await _userManager.IsInRoleAsync(user, RolesName.Employer);
            if (userIsInRole)
            {
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
                return new BaseResponseDto
                {
                    IsSuccess = false,  
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "This task is open for joining. Just joining."
                };
            }
            if (task.Performers.Any(p => p.Id == user.Id))
            {
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
                Status = JoinRequestStatus.Pending,
                Description = dto.Description
            };

            _db.JoinToTaskRequests.Add(joinRequest);
            await _db.SaveChangesAsync();

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "Your request to join the task has been sent to the owner."
            };
        }
    }
}
