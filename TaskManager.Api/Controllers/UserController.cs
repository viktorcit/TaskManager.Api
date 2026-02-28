using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Model;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("/users")]
    public class UserController : ControllerBase
    {
        public readonly AppDbContext _db;

        public UserController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<UserDto>> GetUsers()
        {
            var users = await _db.Users.ToListAsync();

            var response = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Age = u.Age
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var response = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Age = user.Age
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<RegisterUserDto>> CreateUser(RegisterUserDto dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Age = dto.Age
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var response = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Age = user.Age
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
