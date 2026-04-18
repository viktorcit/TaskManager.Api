using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("account")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }



        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> RegisterAsync(RegisterUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(dto);

            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.Conflict => Conflict(result.ResponseMessage),
                ErrorType.InternalServerError => StatusCode(500, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> LoginAsync(LoginUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _logger.LogInformation("Login attempt for user: {Nickname}", dto.Nickname);

            var result = await _authService.LoginAsync(dto);

            return result.ErrorType switch
            {
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }


        [Authorize]
        [HttpPost("request-employer")]
        public async Task<ActionResult<RequestEmployerDto>> RequestEmployerAsync(RequestEmployerDto dto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();
            _logger.LogInformation("User with ID {UserId} is requesting employer status with company name: {CompanyName}", userId, dto.CompanyName);

            var result = await _authService.RequestEmployerAsync(dto, userId);

            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                ErrorType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}

