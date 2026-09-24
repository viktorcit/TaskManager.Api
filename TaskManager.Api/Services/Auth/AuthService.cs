using Microsoft.AspNetCore.Identity;
using System.Data;
using TaskManager.Api.Data.Contracts;
using TaskManager.Api.Data.DTO;
using TaskManager.Api.Data.DTO.RequestDTOs.Auth;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.Auth;

namespace TaskManager.Api.Services.Auth
{
    public class AuthService : IAuthService
    {

        private readonly IJwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IJwtService jwtService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AuthService> logger
            )
        {
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }


        public async Task<BaseResponseDto<string>> RegisterAsync(RegisterRequestDto dto)
        {
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt failed for existing user with userName {UserName} and ID {UserId}", existingUser.UserName, existingUser.Id);
                return ResponseFactory.Fail<string>(ResponseType.Conflict, "UserName is already taken.");
            }

            var user = CreateUserEntity(dto);

            var validatePassword = await _userManager.PasswordValidators.First().ValidateAsync(_userManager, user, dto.Password);
            if (!validatePassword.Succeeded)
            {
                return ResponseFactory.Fail<string>(ResponseType.BadRequest, "Password does not meet the requirements.");
            }

            var createUser = await _userManager.CreateAsync(user, dto.Password);
            if (!createUser.Succeeded)
            {
                var errors = createUser.Errors.Select(e => e.Description).ToList();
                _logger.LogError("User registration failed for userName {UserName}. Errors: {Errors}", dto.UserName, string.Join(", ", errors));
                return ResponseFactory.Fail<string>(ResponseType.InternalServerError, ErrorMessages.ServerError);
            }

            var addRole = await _userManager.AddToRoleAsync(user, RolesName.User);
            if (!addRole.Succeeded)
            {
                _logger.LogError("Failed to assign role to user {UserName} with ID {UserId}. Errors: {Errors}",
                    user.UserName, user.Id, string.Join(", ", addRole.Errors.Select(e => e.Description)));
                await _userManager.DeleteAsync(user);
                return ResponseFactory.Fail<string>(ResponseType.InternalServerError, ErrorMessages.ServerError);
            }

            var tokenData = CreateAccessTokenInfo(user, new List<string> { RolesName.User });
            var token = _jwtService.GenerateToken(tokenData);

            _logger.LogInformation("User {UserName} register successfully with ID {UserId}", user.UserName, user.Id);
            return ResponseFactory.Ok<string>(token);
        }


        public async Task<BaseResponseDto<string>> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed for non-existent user with userName {UserName}", dto.UserName);
                return ResponseFactory.Fail<string>(ResponseType.BadRequest, ErrorMessages.InvalidLoginData);
            }
            var signIn = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (signIn.IsLockedOut)
            {
                _logger.LogWarning("Registration attempt failed for existing user with userName {UserName} and ID {UserId}", user.UserName, user.Id);
                return ResponseFactory.Fail<string>(ResponseType.Forbidden, "Your account is locked due to too many failed login attempts. Please try again later.");
            }
            if (!signIn.Succeeded)
            {
                _logger.LogWarning("Invalid login attempt for user {UserName} with ID {UserId}", user.UserName, user.Id);
                return ResponseFactory.Fail<string>(ResponseType.BadRequest, ErrorMessages.InvalidLoginData);
            }
            var roles = await _userManager.GetRolesAsync(user);

            var tokenData = CreateAccessTokenInfo(user, (IReadOnlyCollection<string>)roles);
            var token = _jwtService.GenerateToken(tokenData);

            _logger.LogInformation("User {UserName} logged in successfully with ID {UserId}", user.UserName, user.Id);
            return ResponseFactory.Ok<string>(token);
        }



        //private static methods
        private static AccessTokenInfo CreateAccessTokenInfo(ApplicationUser user, IReadOnlyCollection<string> roles)
        {
            var tokenData = new AccessTokenInfo
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserRoles = roles
            };
            return tokenData;
        }

        private static ApplicationUser CreateUserEntity(RegisterRequestDto dto)
        {
            var user = new ApplicationUser
            {
                Name = dto.Name,
                UserName = dto.UserName,
                NormalizedUserName = dto.UserName.Normalize(),
                Age = dto.Age,
                CreatedAt = DateTimeOffset.UtcNow
            };
            return user;
        }
    }
}
