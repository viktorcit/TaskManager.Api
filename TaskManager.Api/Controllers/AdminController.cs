using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Enums;
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

        public AdminController(UserManager<ApplicationUser> userManager, IAdminService adminService)
        {
            _userManager = userManager;
            _adminService = adminService;
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("employer-requests")]
        public async Task<ActionResult<List<EmployerRequestSummaryDto>>> GetPendingEmployerRequestsAsync()
        {
            var pendingRequests = await _adminService.GetPendingEmployerRequestsAsync();

            return Ok(pendingRequests);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("employer-requests/{id}")]
        public async Task<ActionResult<EmployerRequestSummaryDto>> GetPendingRequestsByIdAsync(int id)
        {
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
