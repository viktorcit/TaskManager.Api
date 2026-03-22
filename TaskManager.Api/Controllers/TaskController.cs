using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Model;
using System.Security.Claims;
using static TaskManager.Api.Model.JoinToTaskRequest;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("tasks")]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;


        public TaskController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }



        [HttpGet]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetTasksInProgress()
        {
            var tasks = await _db.Tasks
                .Where(t => t.Status == Model.TaskStatus.InProgress)
                .ToListAsync();


            var responseTasks = tasks.Select(t => new TaskItemSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                OwnerUsername = t.OwnerUsername,
                OwnerId = t.OwnerId,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(responseTasks);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTasksInProgressById(int id)
        {
            var task = await _db.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.Status == Model.TaskStatus.InProgress);
            if (task == null)
            {
                return NotFound();
            }

            var responseTasks = new TaskItemSummaryDto
            {
                Id = id,
                Title = task.Title,
                OwnerUsername = task.OwnerUsername,
                OwnerId = task.OwnerId,
                CreatedAt = task.CreatedAt
            };

            return Ok(responseTasks);
        }



        [Authorize(Roles = "Employer")]
        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> CreateTask(CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var owner = await _userManager.GetUserAsync(User);
            var ownerId = _userManager.GetUserId(User);
            if (owner == null || ownerId == null)
            {
                return Unauthorized();
            }

            var performers = await _userManager.Users
                .Where(u => dto.PerformersId.Contains(u.Id))
                .ToListAsync();
            if (performers.Count != dto.PerformersId.Count)
            {
                return BadRequest("One or more performers not found.");
            }

            var ownerUsername = owner.UserName;
            if (ownerUsername == null)
            {
                return BadRequest("Owner username not found.");
            }

            var task = new TaskItem
            {
                Owner = owner,
                OwnerId = ownerId,
                OwnerUsername = ownerUsername,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                CanAnyoneJoin = dto.CanAnyoneJoin,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = Model.TaskStatus.InProgress,
                Performers = performers.ToList(),
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            var responseTask = new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                Status = task.Status,
                OwnerId = task.OwnerId,
                OwnerUsername = task.OwnerUsername,
                Performers = task.Performers.ToList()
            };

            return Ok(responseTask);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            var ownerTaskId = task.OwnerId;
            if (ownerTaskId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid("You can`t delete not yours task.");
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("my-tasks")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetUserTasks()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            var userTasks = await _db.Tasks
                .Where(t => t.Performers.Any(p => p.Id == userId))
                .ToListAsync();
            if (userTasks.Count == 0)
            {
                return Ok(new List<TaskItemSummaryDto>());
            }

            var responseTasks = userTasks.Select(t => new TaskItemSummaryDto
            {
                Title = t.Title,
                OwnerUsername = t.OwnerUsername,
            }).ToList();
            return Ok(responseTasks);
        }

        [Authorize]
        [HttpGet("my-created-tasks")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetUserCreatedTasks()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }
            var userTasks = await _db.Tasks
                .Where(t => t.OwnerId == userId)
                .ToListAsync();
            if (userTasks.Count == 0)
            {
                return NotFound("You have no created tasks.");
            }
            var responseTasks = userTasks.Select(t => new TaskItemSummaryDto
            {
                Title = t.Title,
                OwnerUsername = t.OwnerUsername,
            }).ToList();
            return Ok(responseTasks);
        }


        [Authorize]
        [HttpPost("{id}/join")]
        public async Task<ActionResult> JoinTask([FromRoute(Name = "id")] int taskId)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || userId == null)
            {
                return Unauthorized();
            }
            var userIsInRole = await _userManager.IsInRoleAsync(user, "Employer");
            if (userIsInRole)
            {
                return Forbid("Employers cannot join tasks as performers.");
            }
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }
            if (!task.CanAnyoneJoin)
            {
                return BadRequest("This task is not open for joining. Create request for join.");
            }
            if (task.Performers.Any(p => p.Id == userId))
            {
                return BadRequest("You are already a performer of this task.");
            }

            task.Performers.Add(user);
            await _db.SaveChangesAsync();

            return Ok("You have successfully joined the task.");
        }

        [Authorize]
        [HttpPost("{id}/join-request")]
        public async Task<ActionResult> RequestToJoinTask([FromRoute(Name = "id")] int taskId, JoinToTaskRequest dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if (user == null || userId == null)
            {
                return Unauthorized();
            }
            var userIsInRole = await _userManager.IsInRoleAsync(user, "Employer");
            if (userIsInRole)
            {
                return Forbid("Employers cannot join tasks as performers.");
            }
            var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }
            if (task.CanAnyoneJoin)
            {
                return BadRequest("This task is open for joining. Just joining.");
            }
            if (task.Performers.Any(p => p.Id == userId))
            {
                return BadRequest("You are already a performer of this task.");
            }
            var username = user.UserName;
            if (username == null)
            {
                return BadRequest("Username not found.");
            }


            var joinRequest = new JoinToTaskRequest
            {
                TaskId = taskId,
                UserId = userId,
                UserName = username,
                Task = task,
                Status = JoinRequestStatus.Pending,
                Description = dto.Description
            };

            _db.JoinToTaskRequests.Add(joinRequest);
            await _db.SaveChangesAsync();

            return Ok("Your request to join the task has been sent to the owner.");
        }
    }
}
