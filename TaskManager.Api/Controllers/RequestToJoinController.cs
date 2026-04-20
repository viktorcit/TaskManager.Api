using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.JoinDto;
using TaskManager.Api.Data.DTO.JoinToTaskDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("request-to-task")]
    public class RequestToJoinController : ControllerBase
    {
        private readonly IRequestToJoinService _requestToJoinService;
        private readonly ILogger<RequestToJoinController> _logger;


        public RequestToJoinController(IRequestToJoinService requestToJoinService, ILogger<RequestToJoinController> logger)
        {
            _requestToJoinService = requestToJoinService;
            _logger = logger;
        }


        [Authorize(Roles = "Employer")]
        [HttpGet]
        public async Task<ActionResult<List<JoinToTaskRequestSummaryDto>>> GetJoinToTaskRequestsAsync()
        {
            var ownerId = User.GetUserId();
            if (ownerId == null)
            {
                return Unauthorized();
            }

            var result = await _requestToJoinService.GetJoinToTaskRequestsAsync(ownerId);

            return Ok(result);
        }


        [Authorize(Roles = "Employer")]
        [HttpGet("{id}")]
        public async Task<ActionResult<JoinToTaskRequestSummaryDto>> GetJoinToTaskRequestByIdAsync([FromRoute(Name = "id")] int requestId)
        {
            var ownerId = User.GetUserId();
            if (ownerId == null)
            {
                return Unauthorized();
            }

            var result = await _requestToJoinService.GetJoinToTaskRequestByIdAsync(requestId, ownerId);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }


        [Authorize(Roles = "Employer")]
        [HttpPost("{id}/approve")]
        public async Task<ActionResult> ApproveJoinToTaskRequestAsync([FromRoute(Name = "id")] int requestId)
        {
            var ownerId = User.GetUserId();
            if (ownerId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Employer {UserId} is attempting to approve join request with id {RequestId}", ownerId, requestId);

            var result = await _requestToJoinService.ApproveJoinToTaskRequestAsync(requestId, ownerId);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize(Roles = "Employer")]
        [HttpPost("{id}/reject")]
        public async Task<ActionResult> RejectJoinToTaskRequestAsync([FromRoute(Name = "id")] int requestId)
        {
            var ownerId = User.GetUserId();
            if (ownerId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Employer {UserId} is attempting to reject join request with id {RequestId}", ownerId, requestId);

            var result = await _requestToJoinService.RejectJoinToTaskRequestAsync(requestId, ownerId);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize]
        [HttpPost("{id}/join")]
        public async Task<ActionResult> JoinTaskAsync([FromRoute(Name = "id")] int taskId)
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("User {UserId} is attempting to join task with id {TaskId}", userId, taskId);

            var result = await _requestToJoinService.JoinTaskAsync(taskId, userId);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize]
        [HttpPost("{id}/join-request")]
        public async Task<ActionResult> RequestToJoinTaskAsync([FromRoute(Name = "id")] int taskId, JoinToTaskRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();
            _logger.LogInformation("User {UserId} is attempting to request to join task with id {TaskId}", userId, taskId);

            var result = await _requestToJoinService.RequestToJoinTaskAsync(taskId, dto, userId);

            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
