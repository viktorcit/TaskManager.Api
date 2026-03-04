using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.JWT;
using TaskManager.Api.Model;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwtService;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public AuthController(AppDbContext db, JwtService jwtService, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _passwordHasher = passwordHasher;
            _db = db;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var normalizedNickname = dto.Nickname.ToLower();

            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Nickname.ToLower() == normalizedNickname);

            if (existingUser != null)
            {
                return Conflict("User already exists.");
            }

            var user = new ApplicationUser
            {
                Name = dto.Name,
                Nickname = dto.Nickname,
                Age = dto.Age,
                CreatedAt = DateTimeOffset.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponseDto { Token = token });
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var normalizedNickname = dto.Nickname.ToLower();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Nickname.ToLower() == normalizedNickname);
            if (user == null)
            {
                return Unauthorized("Invalid nickname or password.");
            }
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid nickname or password.");
            }
            var token = _jwtService.GenerateToken(user);
            return Ok(new AuthResponseDto { Token = token });
        }
    }
}
