using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Enums;
using TaskManager.Api.JWT;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Services
{
    public class AuthService : IAuthService
    {

        private readonly JwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _db;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            JwtService jwtService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext db,
            ILogger<AuthService> logger
            )
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _logger = logger;
        }



        public async Task<BaseResponseWithDataDto<AuthResponseDto>> RegisterAsync(RegisterUserDto dto)
        {
            var nickname = dto.Nickname?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return new BaseResponseWithDataDto<AuthResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Nickname is required."
                };
            }
            var existingUser = await _userManager.FindByNameAsync(nickname);
            if (existingUser != null)
            {
                return new BaseResponseWithDataDto<AuthResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Conflict,
                    ResponseMessage = "User already exists."
                };
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
                return new BaseResponseWithDataDto<AuthResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Registration failed",
                    Errors = errors
                };
            }

            var roleResult = await _userManager.AddToRoleAsync(user, RolesName.User);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return new BaseResponseWithDataDto<AuthResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.InternalServerError,
                    ResponseMessage = "Failed to assign role to the user."
                };
            }

            var token = await GenerateTokenAsync(user);
            _logger.LogInformation("User {Nickname} registered successfully with ID {UserId}", user.Nickname, user.Id);

            return new BaseResponseWithDataDto<AuthResponseDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "",
                Data = new AuthResponseDto
                {
                    Token = token,
                    Message = "Account successfully registered"
                }
            };
        }


        public async Task<BaseResponseWithDataDto<AuthResponseDto>> LoginAsync(LoginUserDto dto)
        {
            var nickname = dto.Nickname?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return new BaseResponseWithDataDto<AuthResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "Nickname is required."
                };
            }

            var user = await _userManager.FindByNameAsync(nickname);
            if (user == null)
            {
                return new BaseResponseWithDataDto<AuthResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = "Invalid nickname or password."
                };
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                return new BaseResponseWithDataDto<AuthResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "The number of login attempts has been exceeded, please try again later."
                };
            }
            if (!result.Succeeded)
            {
                return new BaseResponseWithDataDto<AuthResponseDto>
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = "Invalid nickname or password."
                };
            }

            var token = await GenerateTokenAsync(user);
            _logger.LogInformation("User {Nickname} logged in successfully with ID {UserId}", user.Nickname, user.Id);

            return new BaseResponseWithDataDto<AuthResponseDto>
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = "You have successfully logged in",
                Data = new AuthResponseDto
                {
                    Token = token
                }
            };
        }


        public async Task<BaseResponseDto> RequestEmployerAsync(RequestEmployerDto dto, string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) 
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Unauthorized,
                    ResponseMessage = ""
                };
            var isInRole = await _userManager.IsInRoleAsync(user, "Employer");
            if (isInRole)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.Forbidden,
                    ResponseMessage = "You are already an employer"
                };

            var existPending = await _db.EmployerRequests
                .FirstOrDefaultAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending);
            if (existPending != null)
                return new BaseResponseDto
                {
                    IsSuccess = false,
                    ErrorType = ErrorType.BadRequest,
                    ResponseMessage = "You already have a pending employer request."
                };

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
            _logger.LogInformation("User {Nickname} submitted an employer request with ID {RequestId}", user.Nickname, request.Id);

            return new BaseResponseDto
            {
                IsSuccess = true,
                ErrorType = ErrorType.None,
                ResponseMessage = $"You have successfully submitted your request. Your request ID: {request.Id}"
            };
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
