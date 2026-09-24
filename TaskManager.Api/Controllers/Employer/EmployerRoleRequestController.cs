using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.RequestDto.Employer;
using TaskManager.Api.Data.DTO.RequestDto.EmployerDto;
using TaskManager.Api.Data.DTO.RequestDTOs.Employer;
using TaskManager.Api.Data.DTO.ResponseDTOs.Employer;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Interfaces.Employer;

namespace TaskManager.Api.Controllers.Employer
{
    [ApiController]
    [Route("employer-requests")]
    public class EmployerRoleRequestController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmployerRoleRequestService _employerRoleService;
        private readonly ILogger<EmployerRoleRequestController> _logger;

        public EmployerRoleRequestController(
            UserManager<ApplicationUser> userManager,
            IEmployerRoleRequestService employerRoleService,
            ILogger<EmployerRoleRequestController> logger)
        {
            _userManager = userManager;
            _employerRoleService = employerRoleService;
            _logger = logger;
        }

        //GET
        [Authorize(Roles = RolesName.Admin)]
        [HttpGet("pending")]
        public async Task<ActionResult<List<RequestToEmployerRoleDto>>> GetPendingEmployerRequestsAsync()
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} is retrieving pending employer requests.", adminId);

            var pendingRequests = await _employerRoleService.GetPendingEmployerRequestsAsync();
            return Ok(pendingRequests.Data);
        }


        [Authorize(Roles = RolesName.Admin)]
        [HttpGet("pending/{requestId}")]
        public async Task<ActionResult<RequestToEmployerRoleDto>> GetPendingRequestsByIdAsync(int requestId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} is retrieving details for employer request {RequestId}.", adminId, requestId);

            var result = await _employerRoleService.GetPendingRequestsByIdAsync(requestId);

            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                _ => Ok(result.Data)
            };
        }


        //POST
        [Authorize(Roles = RolesName.Admin)]
        [HttpPost("{requestId}/approve")]
        public async Task<ActionResult<ApproveEmployerRoleRequestDto>> ApproveEmployerRequestAsync(int requestId, ApproveEmployerRoleRequestDto dto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} is attempting to approve a request employer role with ID: {RequestId}", adminId, requestId);

            var result = await _employerRoleService.ApproveEmployerRequestAsync(requestId, dto, adminId);

            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(result.ResponseMessage),
                ResponseType.BadRequest => BadRequest(result.ResponseMessage),
                ResponseType.Conflict => Conflict(result.ResponseMessage),
                ResponseType.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }


        [Authorize(Roles = RolesName.Admin)]
        [HttpPost("{id}/reject")]
        public async Task<ActionResult<RejectEmployerRoleRequestDto>> RejectEmployerRequestAsync(int id, RejectEmployerRoleRequestDto dto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} is rejecting employer request {RequestId}.", adminId, id);

            var result = await _employerRoleService.RejectEmployerRequestAsync(id, dto, adminId);
            return result.ResponseType switch
            {
                ResponseType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }


        [Authorize(Roles = RolesName.User)]
        [HttpPost]
        public async Task<ActionResult<CreateEmployerRoleRequestDto>> CreateEmployerRoleRequestAsync(CreateEmployerRoleRequestDto dto)
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException("The Authorized user not found when retrieving via UserManager.");
            _logger.LogInformation("User with ID {UserId} is requesting employer status with company name: {CompanyName}", user.Id, dto.CompanyName);

            var result = await _employerRoleService.CreateEmployerRoleRequestAsync(dto, user);
            return result.ResponseType switch
            {
                ResponseType.Conflict => Conflict(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
