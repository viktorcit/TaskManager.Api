using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
        {
            var adminId = User.GetUserId();
            if (adminId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Admin {AdminId} requested a list of all users", adminId);
            var result = await _userService.GetUsers();

            return result;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUserDto>> GetUserByIdAsync([FromRoute(Name = "id")] string userId)
        {
            var adminId = User.GetUserId();
            if (adminId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Admin with the ID {AdminId} requests user details by ID: {userId}", adminId, userId);
            var result = await _userService.GetUserByIdAsync(userId);
            if (result == null)
            {
                return NotFound("User not found");
            }

            return result;
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync([FromRoute(Name = "id")] string userId)
        {
            var adminId = User.GetUserId();
            if (adminId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Admin with the ID {AdminId} is attempting to delete the user with ID: {UserId}", adminId, userId);
            var result = await _userService.DeleteUserAsync(userId, adminId);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                _ => NoContent()
            };
        }


        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUserAsync([FromRoute(Name = "id")] string userId, AdminUpdateUserDto dto)
        {
            var adminId = User.GetUserId();
            if (adminId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation("Admin {AdminId} is attempting to update the user profile with ID: {UserId}", adminId, userId);
            var result = await _userService.UpdateUserAsync(userId, adminId, dto);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
