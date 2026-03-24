using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Model;

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

        [HttpGet]
        public async Task<ActionResult<UserDto>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var response = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Nickname = u.Nickname,
                UserName = u.UserName,
                Age = u.Age,
                CreatedAt = u.CreatedAt,
            }).ToList();

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id.ToString());
            if (user == null) return NotFound();

            user.PerformerTasks = await _db.Tasks.Where(t => t.Performers.Any(p => p.Id == user.Id)).ToListAsync();

            var response = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Nickname = user.Nickname,
                UserName = user.UserName,
                Age = user.Age,
                CreatedAt = user.CreatedAt,
                PerformerTasks = user.PerformerTasks
            };

            return Ok(response);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _db.Users.FindAsync(id.ToString());

            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDto dto)
        {
            var user = await _db.Users.FindAsync(id.ToString());
            if (user == null) return NotFound();
            user.Name = dto.Name;
            user.Age = dto.Age;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return Ok("Data of user updated");
        }
    }
}
