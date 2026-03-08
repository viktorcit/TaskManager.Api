using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.JWT;
using TaskManager.Api.Model;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("account")]
    public class AuthController : ControllerBase
    {
        const string ROLE_USER = "User";

        private readonly JwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            JwtService jwtService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nickname = dto.Nickname?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return BadRequest("Nickname is required.");
            }
            var existingUser = await _userManager.FindByNameAsync(nickname);
            if (existingUser != null)
            {
                return Conflict("User already exists.");
            }

            var user = new ApplicationUser
            {
                Name = dto.Name,
                UserName = nickname,
                Nickname = nickname,
                Age = dto.Age,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { errors });
            }


            var roleResult = await _userManager.AddToRoleAsync(user, ROLE_USER);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return StatusCode(500, "Failed to assign role to the user.");
            }

            var token =await GenerateToken(user);

            return Ok(new AuthResponseDto { Token = token });
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nickname = dto.Nickname?.Trim() ?? "";
            if(string.IsNullOrWhiteSpace(nickname))
            {
                return BadRequest("Nickname is required.");
            }

            var user = await _userManager.FindByNameAsync(nickname);
            if (user == null)
            {
                return Unauthorized("Invalid nickname or password.");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                return Forbid();
            }
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid nickname or password.");
            }

            var token = await GenerateToken(user);

            return Ok(new AuthResponseDto { Token = token });
        }


        [Authorize]
        [HttpPost("request-employer")]
        public async Task<ActionResult<RequestEmployerDto>> RequestEmployer(RequestEmployerDto dto)
        {
            return dto;
        }



        private async Task<string> GenerateToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = _jwtService.GenerateToken(claims);
            return token;
        }
    }
}

