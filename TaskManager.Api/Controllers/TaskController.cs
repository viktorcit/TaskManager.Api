using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Model;
using System.Security.Claims;

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
                Owner = t.Owner,
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
                Owner = task.Owner,
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
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            var task = new TaskItem
            {
                Owner = owner,
                OwnerId = ownerId,
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
                Owner = task.Owner,
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
        [HttpGet("my-task")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetUserTasks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var userTasks = await _db.Tasks
                .Where(t => t.Performers.Any(p => p.Id == userId))
                .ToListAsync();
            if (userTasks.Count == 0)
            {
                return NotFound("You have no tasks.");
            }

            var responseTasks = userTasks.Select(t => new TaskItemSummaryDto
            {
                Title = t.Title,
                Owner = t.Owner,
            }).ToList();
            return Ok(responseTasks);
        }

        [Authorize]
        [HttpPost("/my-created-tasks")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetUserCreatedTasks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
                Owner = t.Owner,
            }).ToList();
            return Ok(responseTasks);
        }
    }
}
