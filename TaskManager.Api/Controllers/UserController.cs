using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var response = users.Select(u => new AdminUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Nickname = u.Nickname,
                Age = u.Age,
                CreatedAt = u.CreatedAt,
            }).ToList();

            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUserDto>> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id.ToString());
            if (user == null) return NotFound();

            var userPerformerTasks = await _db.Tasks.Where(t => t.Performers.Any(p => p.Id == user.Id))
                .Select(p => new TaskItemShortDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Status = p.Status,
                }).ToListAsync();
            var userOwnerTasks = await _db.Tasks.Where(t => t.Owner.Id == user.Id)
                .Select(p => new TaskItemShortDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Status = p.Status,
                }).ToListAsync();

            var response = new AdminUserDto
            {
                Id = user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                Age = user.Age,
                CreatedAt = user.CreatedAt,
                PerformerTasks = userPerformerTasks,
                OwnerTasks = userOwnerTasks,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd
            };

            return Ok(response);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id)
        {
            var user = await _db.Users.FindAsync(id.ToString());
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, AdminUpdateUserDto dto)
        {
            var user = await _db.Users.FindAsync(id.ToString());
            if (user == null) return NotFound();

            if (dto.Name != null)
                user.Name = dto.Name;
            if (dto.Age != null)
                user.Age = dto.Age;
            if(dto.Nickname != null)
                user.Nickname = dto.Nickname;
            await _db.SaveChangesAsync();
            return Ok($"Data of user {id} updated");
        }
    }
}
