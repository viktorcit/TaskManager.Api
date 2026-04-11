using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Model;
using TaskManager.Api.Data.DTO.UserDto;

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


        //Get
        [HttpGet]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetTasksInProgressAsync()
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

            return Ok(responseTasks);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetAllTasksAsync()
        {
            var tasks = await _db.Tasks.ToListAsync();

            var responseTasks = tasks.Select(t => new TaskItemSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                OwnerUsername = t.OwnerUsername,
                Status = t.Status
            }).ToList();

            return Ok(responseTasks);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTaskByIdAsync(int id)
        {
            var task = await _db.Tasks
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound();
            }

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

            return Ok(responseTasks);
        }


        [Authorize(Roles = "Employer")]
        [HttpGet("my-created")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetUserCreatedTasksAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }
            var userTasks = await _db.Tasks
                .Where(t => t.OwnerId == user.Id)
                .ToListAsync();
            if (userTasks.Count == 0)
            {
                return NotFound("You have no created tasks.");
            }
            var responseTasks = userTasks.Select(t => new TaskItemSummaryDto
            {
                Id = t.Id,
                Title = t.Title,
                OwnerUsername = t.OwnerUsername,
            }).ToList();
            return Ok(responseTasks);
        }

        [Authorize]
        [HttpGet("my-performing")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetUserPerformingTasksAsync()
        {
            var performer = await GetCurrentUserAsync();
            if (performer == null)
            {
                return Unauthorized();
            }
            var isInRole = await _userManager.IsInRoleAsync(performer, "Employer");
            if (isInRole)
            {
                return Forbid("Only a regular user can access their tasks.");
            }
            var tasks = await _db.Tasks.Where(t => t.Performers.Contains(performer)).ToListAsync();
            if (tasks.Count == 0)
            {
                return NotFound("You have no tasks!");
            }

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

        [Authorize]
        [HttpGet("my-performing/{id}")]
        public async Task<ActionResult<TaskItemDto>> GetUserPerformingTaskByIdAsync(int id)
        {
            var performer = await GetCurrentUserAsync();
            if (performer == null)
            {
                return Unauthorized();
            }
            var isInRole = await _userManager.IsInRoleAsync(performer, "Employer");
            if (isInRole)
            {
                return Forbid("Only a regular user can access their tasks.");
            }
            var task = await _db.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.Performers.Any(p => p.Id == performer.Id));
            if (task == null)
            {
                return NotFound();
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
            return Ok(responseTask);
        }


        //Post
        [Authorize(Roles = "Employer")]
        [HttpPost("create")]
        public async Task<ActionResult<TaskItemResponseDto>> CreateTaskAsync(CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var owner = await GetCurrentUserAsync();
            if (owner == null)
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

            return Ok(responseTask);
        }


        

        //Delete
        [Authorize(Roles = "Employer")]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteTaskAsync(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            if (task.OwnerId != user.Id)
            {
                return Forbid("You can`t delete not yours task.");
            }

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            return NoContent();
        }


        //Private methods
        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = _userManager.GetUserId(User);
            return userId == null ? null : await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
