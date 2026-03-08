using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;

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
        public async Task<ActionResult> GetPendingEmployerRequests()
        {
            var pendingRequests = await _db.EmployerRequests
                .Where(r => r.Status == PENDING)
                .ToListAsync();
            return Ok(pendingRequests);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("employer-requests/{id}")]
        public async Task<ActionResult<RequestEmployerDto>> GetPendingRequestsById(int id)
        {
            var request = await _db.EmployerRequests
                .Where(r => r.Status == PENDING)
                .FirstOrDefaultAsync(r => r.Id == id);
            return Ok(request);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("employer-requests/{id}/approve")]
        public async Task<ActionResult<ApproveEmployerDto>> ApproveEmployerRequest(int id, ApproveEmployerDto dto)
        {
            var request = await _db.EmployerRequests.FindAsync(id);
            if (request == null) return NotFound();
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
