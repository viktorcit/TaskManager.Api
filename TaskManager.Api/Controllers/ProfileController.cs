using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.ProfileDto;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Model;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("profile")]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public ProfileController(UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }




        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublicProfileDto>>> GetAllProfilesAsync(int page = 1, int pageSize = 20)
        {
            var profiles = await _userManager.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = profiles.Select(u => new PublicProfileDto
            {
                Id = u.Id,
                Name = u.Name,
                Nickname = u.Nickname
            }).ToList();

            return Ok(response);
        }

        [Authorize]
        [HttpGet("{nickname}")]
        public async Task<ActionResult<PublicProfileDto>> GetProfileOfUserAsync(string nickname)
        {
            var findUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Nickname.ToLower() == nickname.ToLower());
            if(findUser == null) return NotFound("User not found");

            var response = new PublicProfileDto
            {
                Id = findUser.Id,
                Nickname = findUser.Nickname,
                Name = findUser.Name,
            };
            return Ok(response);
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<ActionResult<PrivateProfileDto>> GetProfileAsync()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);
            if (userId == null || user == null)
            {
                return Unauthorized("Profile not found");
            }
            var userPerformerTasks = await _db.Tasks
                .Where(t => t.Performers.Any(p => p.Id == user.Id))
                .Select(t => new TaskItemShortDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status
                }).ToListAsync();
            var userOwnerTasks = await _db.Tasks
                .Where(t => t.Owner.Id == user.Id)
                .Select(t => new TaskItemShortDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status
                }).ToListAsync();


            var response = new PrivateProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                Age = user.Age,
                CreatedAt = user.CreatedAt,
                PerformerTasks = userPerformerTasks,
                OwnerTasks = userOwnerTasks
            };

            return Ok(response);
        }


        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteProfileAsync()
        {
            var profile = await _userManager.GetUserAsync(User);
            if (profile == null) return Unauthorized();

            var isInRole = await _userManager.IsInRoleAsync(profile, "Admin");
            if (isInRole)
            {
                return Forbid("You can't delete admin account");
            }

            await _userManager.DeleteAsync(profile);
            return NoContent();
        }


        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> UpdateProfileAsync(UpdateProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = _userManager.GetUserId(User);
            var profile = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userId == null || profile == null) return Unauthorized();

            var isInRole = await _userManager.IsInRoleAsync(profile, "Admin");
            if (isInRole)
            {
                return Forbid("You can't patch admin account");
            }

            if (dto.Name == null && dto.Age == null)
                return BadRequest("Nothing to update");

            if (dto.Name != null)
                profile.Name = dto.Name;
            if (dto.Age != null)
                profile.Age = dto.Age;

            await _userManager.UpdateAsync(profile);
            await _db.SaveChangesAsync();
            return Ok(new UpdateProfileDto
            {
                Name = dto.Name,
                Age = dto.Age
            });
        }


    }
}
