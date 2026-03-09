using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.EmployerDto;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        private const string APPROVED = "Approved";
        private const string PENDING = "Pending";
        private const string REJECTED = "Rejected";




        [Authorize(Roles = "Admin")]
        [HttpGet("employer-requests")]
        public async Task<ActionResult<List<EmployerRequestSummaryDto>>> GetPendingEmployerRequests()
        {
            var pendingRequests = await _db.EmployerRequests
                .Where(r => r.Status == PENDING)
                .ToListAsync();

            var requestsResponse = pendingRequests.Select(r => new EmployerRequestSummaryDto
            {
                Id = r.Id,
                UserId = r.UserId,
                CompanyName = r.CompanyName,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Status = r.Status
            }).ToList();

            return Ok(requestsResponse);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("employer-requests/{id}")]
        public async Task<ActionResult<RequestEmployerDto>> GetPendingRequestsById(int id)
        {
            var request = await _db.EmployerRequests
                .FirstOrDefaultAsync(r => r.Id == id || r.Status == PENDING);
            if(request == null) return NotFound();

            var requestResponse = new EmployerRequestSummaryDto
            {
                Id = request.Id,
                UserId = request.UserId,
                CompanyName = request.CompanyName,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                Status = request.Status
            };

            return Ok(requestResponse);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("employer-requests/{id}/approve")]
        public async Task<ActionResult<ApproveEmployerDto>> ApproveEmployerRequest(int id, ApproveEmployerDto dto)
        {
            var request = await _db.EmployerRequests.FindAsync(id);
            if (request == null) return NotFound();
            if(request.Status != PENDING) return BadRequest("Only pending requests can be approved.");
            request.Status = APPROVED;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("employer-requests/{id}/reject")]
        public async Task<ActionResult<RejectEmployerDto>> RejectEmployerRequest(int id, RejectEmployerDto dto)
        {
            var request = await _db.EmployerRequests.FindAsync(id);
            if (request == null) return NotFound();
            request.Status = REJECTED;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
