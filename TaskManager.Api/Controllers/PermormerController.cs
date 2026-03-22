using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Model;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("performer")]
    public class PermormerController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermormerController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        [Authorize]
        [HttpGet("task")]
        public async Task<ActionResult<TaskItemSummaryDto>> GetTasksOfPerfomer()
        {
            var performer = await _userManager.GetUserAsync(User);
            if (performer == null)
            {
                return NotFound();
            }
            var isInRole = await _userManager.IsInRoleAsync(performer, "Employer");
            if (isInRole)
            {
                return Forbid("Only a regular user can access their tasks.");
            }
            var tasks = _db.Tasks.Where(t => t.Performers.Contains(performer)).ToList();
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
        [HttpGet("task/{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTasksOfPerfomerById(int id)
        {
            var performer = await _userManager.GetUserAsync(User);
            if (performer == null)
            {
                return NotFound();
            }
            var isInRole = await _userManager.IsInRoleAsync(performer, "Employer");
            if (isInRole)
            {
                return Forbid("Only a regular user can access their tasks.");
            }
            var task = _db.Tasks.FirstOrDefault(t => t.Id == id && t.Performers.Contains(performer));
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
    }
}
