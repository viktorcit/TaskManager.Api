using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.RequestDTOs.Task;
using TaskManager.Api.Data.DTO.ResponseDTOs.Tasks;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.Task;

namespace TaskManager.Api.Controllers.Task
{
    [ApiController]
    [Route("tasks")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TaskController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;


        public TaskController(ITaskService taskService, ILogger<TaskController> logger, UserManager<ApplicationUser> userManager)
        {
            _taskService = taskService;
            _logger = logger;
            _userManager = userManager;
        }


        //GET
        [HttpGet]
        public async Task<ActionResult<List<TaskSummaryResponseDto>>> GetTasksInProgressAsync()
        {
            var tasks = await _taskService.GetTasksInProgressAsync();
            return Ok(tasks);
        }


        [HttpGet("{taskId}")]
        public async Task<ActionResult<TaskResponseDto>> GetTaskByIdAsync(int taskId)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }


        [Authorize(Roles = RolesName.Employer)]
        [HttpGet("my-created")]
        public async Task<ActionResult<List<TaskSummaryResponseDto>>> GetUserCreatedTasksAsync()
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(
                    ErrorMessages.AuthenticatedUserNotFound);

            var result = await _taskService.GetUserCreatedTasksAsync(user);
            return Ok(result.Data);
        }


        [Authorize]
        [HttpGet("my-performing")]
        public async Task<ActionResult<List<TaskSummaryResponseDto>>> GetUserPerformingTasksAsync()
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(
                    ErrorMessages.AuthenticatedUserNotFound);

            var result = await _taskService.GetUserPerformingTasksAsync(user);
            return result.ResponseType switch
            {
                ResponseType.NoContent => NoContent(),
                ResponseType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }


        [Authorize]
        [HttpGet("my-performing/{taskId}")]
        public async Task<ActionResult<TaskResponseDto>> GetUserPerformingTaskByIdAsync(int taskId)
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(
                    ErrorMessages.AuthenticatedUserNotFound);

            var result = await _taskService.GetUserPerformingTaskByIdAsync(taskId, user);
            return result.ResponseType switch
            {
                ResponseType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.ResponseMessage),
                ResponseType.NotFound => NotFound(),
                _ => Ok(result.Data)
            };
        }


        //POST
        [Authorize(Roles = RolesName.Employer)]
        [HttpPost("create")]
        public async Task<ActionResult<TaskResponseDto>> CreateTaskAsync(CreateTaskRequestDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            _logger.LogInformation("Employer {UserId} is attempting to create a task with title: {Title}", user.Id, dto.Title);

            var result = await _taskService.CreateTaskAsync(dto, user);
            return result.ResponseType switch
            {
                ResponseType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }


        //DELETE
        [Authorize(Roles = RolesName.Employer)]
        [HttpDelete("delete/{taskId}")]
        public async Task<ActionResult> DeleteTaskAsync(int taskId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Employer {UserId} is attempting to delete task with id: {TaskId}", userId, taskId);

            var result = await _taskService.DeleteTaskAsync(taskId, userId);
            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
