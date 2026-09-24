using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.RequestDTOs.Admin;
using TaskManager.Api.Data.DTO.ResponseDTOs.Admin;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.User;

namespace TaskManager.Api.Controllers.User
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


        //GET
        [Authorize(Roles = RolesName.Admin)]
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserDataResponseDto>> GetUserByIdAsync(string userId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin with the ID {AdminId} requests user details by ID: {userId}", adminId, userId);

            var result = await _userService.GetUserByIdAsync(userId);
            if (result == null)
                return NotFound();

            return result;
        }


        //PATCH
        [Authorize(Roles = RolesName.Admin)]
        [HttpPatch("{userId}")]
        public async Task<IActionResult> UpdateUserAsync(string userId, UpdateUserRequestDto dto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} is attempting to update the user profile with ID: {UserId}", adminId, userId);

            var result = await _userService.UpdateUserAsync(userId, adminId, dto);
            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                ResponseType.BadRequest => BadRequest(result.ResponseMessage),
                ResponseType.Forbidden => Forbid(),
                _ => Ok(result.ResponseMessage)
            };
        }


        //DELETE
        [Authorize(Roles = RolesName.Admin)]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin with the ID {AdminId} is attempting to delete the user with ID: {UserId}", adminId, userId);

            var result = await _userService.DeleteUserAsync(userId, adminId);

            return result.ResponseType switch
            {
                ResponseType.NotFound => NotFound(),
                ResponseType.Forbidden => Forbid(),
                _ => NoContent()
            };
        }
    }
}
