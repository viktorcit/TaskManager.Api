using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Enums;
using TaskManager.Api.JWT;
using TaskManager.Api.Model;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("account")]
    public class AuthController : ControllerBase
    {

        private readonly JwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _db;

        public AuthController(
            JwtService jwtService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext db
            )
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }



        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> RegisterAsync(RegisterUserDto dto)
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


            var roleResult = await _userManager.AddToRoleAsync(user, RolesName.User);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return StatusCode(500, "Failed to assign role to the user.");
            }

            var token = await GenerateTokenAsync(user);

            return Ok(new AuthResponseDto { Token = token });
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> LoginAsync(LoginUserDto dto)
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

            var token = await GenerateTokenAsync(user);

            return Ok(new AuthResponseDto { Token = token });
        }


        [Authorize]
        [HttpPost("request-employer")]
        public async Task<ActionResult<RequestEmployerDto>> RequestEmployerAsync(RequestEmployerDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var isInRole = await _userManager.IsInRoleAsync(user, "Employer");
            if (isInRole)
            {
                return Forbid("you are already an employer");
            }

            var existPending = await _db.EmployerRequests
                .FirstOrDefaultAsync(r => r.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (existPending != null)
            {
                return BadRequest("You already have a pending employer request.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(User.Identity == null || userId == null)
            {
                return Unauthorized();
            }

            var request = new EmployerRequest
            {
                CompanyName = dto.CompanyName,
                Description = dto.Description,
                Website = dto.Website,
                UserId = userId,
                Status = RequestStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.EmployerRequests.Add(request);
            await _db.SaveChangesAsync();

            return Ok(new RequestEmployerResponseDto { Id = request.Id });
        }



        private async Task<string> GenerateTokenAsync(ApplicationUser user)
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

