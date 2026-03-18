using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Model;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmployerProfileService _profileService;

        public AdminController(AppDbContext db, UserManager<ApplicationUser> userManager, EmployerProfileService profileService)
        {
            _db = db;
            _userManager = userManager;
            _profileService = profileService;
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
        public async Task<ActionResult<EmployerRequestSummaryDto>> GetPendingRequestsById(int id)
        {
            var request = await _db.EmployerRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.Status == PENDING);
            if (request == null) return NotFound();

            var requestResponse = new EmployerRequestSummaryDto
            {
                Id = request.Id,
                UserId = request.UserId,
                CompanyName = request.CompanyName,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                Status = request.Status,
                Description = request.Description
            };

            return Ok(requestResponse);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("employer-requests/{id}/approve")]
        public async Task<ActionResult<ApproveEmployerDto>> ApproveEmployerRequest([FromRoute(Name = "id")] int requestId, ApproveEmployerDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _db.EmployerRequests.FindAsync(requestId);
            if (request == null) return NotFound();
            if (request.Status == APPROVED) return BadRequest("This request has already been approved.");
            if (request.Status != PENDING) return BadRequest("Only pending requests can be approved.");

            var user = await _db.Users.FindAsync(request.UserId);
            if (user == null)
            {
                request.Status = REJECTED;
                request.AdminComment = "User not found for this request.";
                request.ReviewedAt = DateTimeOffset.UtcNow;
                request.ReviewedBy = adminId;
                await _db.SaveChangesAsync();
                return NotFound("User not found for this request.");
            }

            var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
            if (isEmployer)
            {
                request.Status = APPROVED;
                request.ReviewedBy = adminId;
                request.ReviewedAt = DateTimeOffset.UtcNow;
                request.AdminComment = dto.AdminComment;
                await _db.SaveChangesAsync();
                return Ok("User already has Employer role — request marked approved");
            }

            var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var existingProfile = await _profileService.GetProfileByUserId(user.Id);
                if (existingProfile == null)
                {
                    var profile = _profileService.CreateProfile(user.Id, new EmployerRequest
                    {
                        UserId = user.Id,
                        CompanyName = request.CompanyName,
                        Website = request.Website,
                        Description = request.Description,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                    await _db.EmployerProfiles.AddAsync(profile);
                }
                else
                {
                    existingProfile.UpdateFromRequest(new EmployerRequest
                    {
                        CompanyName = request.CompanyName,
                        Website = request.Website,
                        Description = request.Description,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Employer");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Failed to assign Employer role to the user.");
                }

                request.Status = APPROVED;
                request.ReviewedBy = adminId;
                request.ReviewedAt = DateTimeOffset.UtcNow;
                request.AdminComment = dto.AdminComment;
                request.UpdatedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while processing the request.");
            }
            return Ok(new
            {
                request.Status,
                request.Id,
                request.UserId,
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("employer-requests/{id}/reject")]
        public async Task<ActionResult<RejectEmployerDto>> RejectEmployerRequest(int id, RejectEmployerDto dto)
        {
            var request = await _db.EmployerRequests.FindAsync(id);
            if (request == null) return NotFound();
            request.Status = REJECTED;
            request.AdminComment = dto.reason;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            request.ReviewedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
