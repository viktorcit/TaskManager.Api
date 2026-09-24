using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.RequestDTOs.Auth;
using TaskManager.Api.Enums;
using TaskManager.Api.Interfaces.Auth;

namespace TaskManager.Api.Controllers.User
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }


        //POST
        [HttpPost("register")]
        public async Task<ActionResult> RegisterAsync(RegisterRequestDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            return result.ResponseType switch
            {
                ResponseType.Conflict => Conflict(result.ResponseMessage),
                ResponseType.BadRequest => BadRequest(result.ResponseMessage),
                ResponseType.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }


        [HttpPost("login")]
        public async Task<ActionResult> LoginAsync(LoginRequestDto dto)
        {
            _logger.LogInformation("Login attempt for user: {Nickname}", dto.UserName);
            var result = await _authService.LoginAsync(dto);

            return result.ResponseType switch
            {
                ResponseType.BadRequest => BadRequest(result.ResponseMessage),
                ResponseType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.ResponseMessage),
                _ => Ok(result.Data)
            };
        }
    }
}

