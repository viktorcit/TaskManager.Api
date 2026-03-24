using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.JoinToTaskDto;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Model;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("owner")]
    public class OwnerController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;


        public OwnerController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        [Authorize(Roles = "Employer")]
        [HttpGet("requests-join-to-task")]
        public async Task<ActionResult<List<JoinToTaskRequestSummaryDto>>> GetJoinToTaskRequests()
        {
            var ownerId = _userManager.GetUserId(User);
            var tasks = await _db.Tasks
                .Where(t => t.OwnerId == ownerId)
                .Select(t => t.Id)
                .ToListAsync();
            if (!tasks.Any())
            {
                return Ok(new List<JoinToTaskRequestSummaryDto>());
            }
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

            return Ok(requests);
        }


        [Authorize(Roles = "Employer")]
        [HttpGet("requests-join-to-task/{id}")]
        public async Task<ActionResult<JoinToTaskRequestSummaryDto>> GetJoinToTaskRequestById(int id)
        {
            var ownerId = _userManager.GetUserId(User);
            if (ownerId == null)
            {
                return Unauthorized();
            }
            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.Task.OwnerId == ownerId);
            if (request == null)
            {
                return NotFound();
            }

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

            return Ok(responseRequest);
        }


        [Authorize(Roles = "Employer")]
        [HttpPost("requests-join-to-task/{id}/approve")]
        public async Task<ActionResult> ApproveJoinToTaskRequest(int id)
        {
            var ownerId = _userManager.GetUserId(User);
            if (ownerId == null)
            {
                return Unauthorized();
            }

            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.Task.OwnerId == ownerId);
            if (request == null)
            {
                return NotFound();
            }
            if(request.Status == JoinToTaskRequest.JoinRequestStatus.Approved)
            {
                return BadRequest("Request has been Approved.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userExistsInTask = await _db.Tasks
                .Where(t => t.Id == request.TaskId)
                .AnyAsync(t => t.Performers.Contains(user));
            if (userExistsInTask)
            {
                return BadRequest("User is already a performer of this task.");
            }

            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            request.Status = JoinToTaskRequest.JoinRequestStatus.Approved;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            task.Performers.Add(user);

            await _db.SaveChangesAsync();
            return Ok("Request has been approved.");
        }

        [Authorize(Roles = "Employer")]
        [HttpPost("requests-join-to-task/{id}/reject")]
        public async Task<ActionResult> RejectJoinToTaskRequest(int id)
        {
            var ownerId = _userManager.GetUserId(User);
            if (ownerId == null)
            {
                return Unauthorized();
            }
            var request = await _db.JoinToTaskRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.Task.OwnerId == ownerId);
            if (request == null)
            {
                return NotFound();
            }
            request.Status = JoinToTaskRequest.JoinRequestStatus.Rejected;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
            return Ok("Request has been rejected.");
        }
    }
}
