using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Data.DTO.TasksDto;
using TaskManager.Api.Data.DTO.UserDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
        {
            var result = await _userService.GetUsers();

            return result;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminUserDto>> GetUserByIdAsync(string userId)
        {
            var result = await _userService.GetUserByIdAsync(userId);
            if (result == null) 
                return NotFound();

            return result;
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            var result = await _userService.DeleteUserAsync(userId);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                _ => NoContent()
            };
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUserAsync(string userId, AdminUpdateUserDto dto)
        {
            var result = await _userService.UpdateUserAsync(userId, dto);

            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }
    }
}
