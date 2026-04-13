using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;


        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }


        //Get
        [HttpGet]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetTasksInProgressAsync()
        {
            var tasks = await _taskService.GetTasksInProgressAsync();

            return Ok(tasks);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetAllTasksAsync()
        {
            var tasks = await _taskService.GetAllTasksAsync();

            return Ok(tasks);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTaskByIdAsync(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            return Ok(task);
        }


        [Authorize(Roles = "Employer")]
        [HttpGet("my-created")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetUserCreatedTasksAsync()
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _taskService.GetUserCreatedTasksAsync(userId);

            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(),
                _ => Ok(result.Data)
            };
        }

        [Authorize]
        [HttpGet("my-performing")]
        public async Task<ActionResult<List<TaskItemSummaryDto>>> GetUserPerformingTasksAsync()
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _taskService.GetUserPerformingTasksAsync(userId);

            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }

        [Authorize]
        [HttpGet("my-performing/{id}")]
        public async Task<ActionResult<TaskItemDto>> GetUserPerformingTaskByIdAsync(int id)
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _taskService.GetUserPerformingTaskByIdAsync(id, userId);

            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.Data)
            };
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
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _taskService.CreateTaskAsync(dto, userId);

            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(),
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }


        

        //Delete
        [Authorize(Roles = "Employer")]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteTaskAsync(int id)
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await _taskService.DeleteTaskAsync(id, userId);

            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
