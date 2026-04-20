using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            IAdminService adminService,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _adminService = adminService;
            _logger = logger;
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("employer-requests")]
        public async Task<ActionResult<List<EmployerRequestSummaryDto>>> GetPendingEmployerRequestsAsync()
        {
            var adminId = User.GetUserId();
            if (adminId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Admin {AdminId} is retrieving pending employer requests.", adminId);
            var pendingRequests = await _adminService.GetPendingEmployerRequestsAsync();

            return Ok(pendingRequests);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("employer-requests/{id}")]
        public async Task<ActionResult<EmployerRequestSummaryDto>> GetPendingRequestsByIdAsync(int id)
        {
            var adminId = User.GetUserId();
            if(adminId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Admin {AdminId} is retrieving details for employer request {RequestId}.", adminId, id);
            var request = await _adminService.GetPendingRequestsByIdAsync(id);

            if (request == null)
            {
                return NotFound("Request not found.");
            }

            return Ok(request);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("employer-requests/{id}/approve")]
        public async Task<ActionResult<ApproveEmployerDto>> ApproveEmployerRequestAsync([FromRoute(Name = "id")] int requestId, ApproveEmployerDto dto)
        {
            var adminId = _userManager.GetUserId(User);
            if (adminId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Admin {AdminId} is attempting to approve a request employer role with ID: {RequestId}", adminId, requestId);

            var result = await _adminService.ApproveEmployerRequestAsync(requestId, dto, adminId);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.Conflict => Conflict(result.ResponseMessage),
                ErrorType.InternalServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("employer-requests/{id}/reject")]
        public async Task<ActionResult<RejectEmployerDto>> RejectEmployerRequestAsync(int id, RejectEmployerDto dto)
        {
            var adminId = _userManager.GetUserId(User);
            if (adminId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Admin {AdminId} is rejecting employer request {RequestId}.", adminId, id);

            var result = await _adminService.RejectEmployerRequestAsync(id, dto, adminId);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
