using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.RequestDTOs.JoinToTask;
using TaskManager.Api.Data.DTO.ResponseDTOs.Tasks;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.Task;

namespace TaskManager.Api.Controllers.Task
{
    [ApiController]
    [Route("request-to-task")]
    public class RequestToJoinTaskController : ControllerBase
    {
        private readonly IRequestToJoinTaskService _requestToJoinService;
        private readonly ILogger<RequestToJoinTaskController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestToJoinTaskController
            (IRequestToJoinTaskService requestToJoinService,
            ILogger<RequestToJoinTaskController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _requestToJoinService = requestToJoinService;
            _logger = logger;
            _userManager = userManager;
        }


        //GET
        [Authorize(Roles = RolesName.Employer)]
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<List<RequestToJoinTaskSummaryResponseDto>>> GetJoinToTaskRequestsAsync(int taskId)
        {
            var ownerId = User.GetUserId();

            var result = await _requestToJoinService.GetJoinToTaskRequestsAsync(taskId, ownerId);
            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                _ => Ok(result.Data)
            };
        }


        [Authorize(Roles = RolesName.Employer)]
        [HttpGet("request/{requestId}")]
        public async Task<ActionResult<RequestToJoinTaskResponseDto>> GetJoinToTaskRequestByIdAsync(int requestId)
        {
            var ownerId = User.GetUserId();

            var result = await _requestToJoinService.GetJoinToTaskRequestByIdAsync(requestId, ownerId);

            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                _ => Ok(result.Data)
            };
        }


        //POST
        [Authorize(Roles = RolesName.Employer)]
        [HttpPost("approve/{requestId}")]
        public async Task<ActionResult> ApproveJoinToTaskRequestAsync(int requestId)
        {
            var ownerId = User.GetUserId();
            _logger.LogInformation("Employer {UserId} is attempting to approve join request with id {RequestId}", ownerId, requestId);

            var result = await _requestToJoinService.ApproveJoinToTaskRequestAsync(requestId, ownerId);

            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                ResponseType.Conflict => Conflict(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }


        [Authorize(Roles = RolesName.Employer)]
        [HttpPost("reject/{requestId}")]
        public async Task<ActionResult> RejectJoinToTaskRequestAsync(int requestId)
        {
            var ownerId = User.GetUserId();
            _logger.LogInformation("Employer {UserId} is attempting to reject join request with id {RequestId}", ownerId, requestId);

            var result = await _requestToJoinService.RejectJoinToTaskRequestAsync(requestId, ownerId);
            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                ResponseType.Conflict => Conflict(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }


        [Authorize]
        [HttpPost("join/{taskId}")]
        public async Task<ActionResult> JoinToTaskAsync(int taskId)
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(ErrorMessages.AuthenticatedUserNotFound);
            _logger.LogInformation("User {UserId} is attempting to join task with id {TaskId}", user.Id, taskId);

            var result = await _requestToJoinService.JoinTaskAsync(taskId, user);
            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                ResponseType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.ResponseMessage),
                ResponseType.Conflict => Conflict(),
                _ => Ok(result.ResponseMessage)
            };
        }


        [Authorize]
        [HttpPost("join-request/{taskId}")]
        public async Task<ActionResult> CreateToJoinTaskRequestAsync(int taskId, JoinToTaskRequestDto dto)
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(ErrorMessages.AuthenticatedUserNotFound);
            _logger.LogInformation("User {UserId} is attempting to request to join task with id {TaskId}", user.Id, taskId);

            var result = await _requestToJoinService.CreateJoinRequestAsync(taskId, dto, user);
            return result.ResponseType switch
            {
                ResponseType.Conflict => Conflict(),
                ResponseType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.ResponseMessage),
                ResponseType.NotFound => NotFound(),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
