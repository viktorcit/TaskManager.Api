using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Data.DTO.UserDto;
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



        const string ROLE_USER = "User";
        const string PENDING_STATUS = "Pending";



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

            var token = await GenerateToken(user);

            return Ok(new AuthResponseDto { Token = token });
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginUserDto dto)
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

            var token = await GenerateToken(user);

            return Ok(new AuthResponseDto { Token = token });
        }


        [Authorize]
        [HttpPost("request-employer")]
        public async Task<ActionResult<RequestEmployerDto>> RequestEmployer(RequestEmployerDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existPending = await _db.EmployerRequests
                .FirstOrDefaultAsync(r => r.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (existPending != null)
            {
                return BadRequest("You already have a pending employer request.");
            }

            var request = new EmployerRequest
            {
                CompanyName = dto.CompanyName,
                Description = dto.Description,
                Website = dto.Website,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Status = PENDING_STATUS,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.EmployerRequests.Add(request);
            await _db.SaveChangesAsync();

            return Ok(new RequestEmployerResponseDto { Id = request.Id });
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

