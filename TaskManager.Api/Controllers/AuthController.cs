using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.EmployerDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.JWT;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("account")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
            {
                return BadRequest(ModelState);
            }

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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (userId == null) 
                return Unauthorized();

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

