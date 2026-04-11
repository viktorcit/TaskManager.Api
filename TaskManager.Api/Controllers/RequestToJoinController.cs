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
    [Route("request-to-join")]
    public class RequestToJoinController : ControllerBase
    {
        private readonly IRequestToJoinService _requestToJoinService;


        public RequestToJoinController(IRequestToJoinService requestToJoinService)
        {
            _requestToJoinService = requestToJoinService;
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
        public async Task<ActionResult<JoinToTaskRequestSummaryDto>> GetJoinToTaskRequestByIdAsync(int id)
        {
            var ownerId = User.GetUserId();
            if (ownerId == null)
            {
                return Unauthorized();
            }

            var result = await _requestToJoinService.GetJoinToTaskRequestByIdAsync(id, ownerId);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }


        [Authorize(Roles = "Employer")]
        [HttpPost("{id}/approve")]
        public async Task<ActionResult> ApproveJoinToTaskRequestAsync(int id)
        {
            var ownerId = User.GetUserId();
            if (ownerId == null)
            {
                return Unauthorized();
            }

            var result = await _requestToJoinService.ApproveJoinToTaskRequestAsync(id, ownerId);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize(Roles = "Employer")]
        [HttpPost("{id}/reject")]
        public async Task<ActionResult> RejectJoinToTaskRequestAsync(int id)
        {
            var ownerId = User.GetUserId();
            if (ownerId == null)
            {
                return Unauthorized();
            }
            var result = await _requestToJoinService.RejectJoinToTaskRequestAsync(id, ownerId);

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
        [HttpPost("{id}join-request")]
        public async Task<ActionResult> RequestToJoinTaskAsync([FromRoute(Name = "id")] int taskId, JoinToTaskRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

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
