using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.RequestDTOs.Profile;
using TaskManager.Api.Data.DTO.ResponseDTOs.Profile;
using TaskManager.Api.Entity;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Helpers;
using TaskManager.Api.Interfaces.User;

namespace TaskManager.Api.Controllers.User
{
    [ApiController]
    [Route("profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<ProfileController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(IProfileService profileService, ILogger<ProfileController> logger, UserManager<ApplicationUser> userManager)
        {
            _profileService = profileService;
            _logger = logger;
            _userManager = userManager;
        }


        //GET
        [Authorize]
        [HttpGet("{userName}")]
        public async Task<ActionResult<PublicProfileResponseDto>> GetProfileOfUserAsync(string userName)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("A user with an ID {UserId} requests a user profile by nickname: {UserName}", userId, userName);

            var result = await _profileService.GetProfileByUsernameAsync(userName);
            if (result == null)
                return NotFound();

            return result;
        }


        [Authorize]
        [HttpGet("my")]
        public async Task<ActionResult<PrivateProfileResponseDto>> GetProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(ErrorMessages.AuthenticatedUserNotFound);

            var result = await _profileService.GetProfileAsync(user)
                ?? throw new InvalidOperationException(ErrorMessages.AuthenticatedUserNotFound);

            return result;
        }


        //PATCH
        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> UpdateProfileAsync(UpdateProfileRequestDto dto)
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(ErrorMessages.AuthenticatedUserNotFound);
            _logger.LogInformation("A user with an ID {UserId} requests to update their profile", user.Id);

            var result = await _profileService.UpdateProfileAsync(dto, user);

            return result.ResponseType switch
            {
                ResponseType.Forbidden => Forbid(),
                ResponseType.BadRequest => BadRequest(result.ResponseMessage),
                _ => Ok(result.ResponseMessage)
            };
        }


        //DELETE
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User)
                ?? throw new InvalidOperationException(ErrorMessages.AuthenticatedUserNotFound);
            _logger.LogInformation("A user with an ID {UserId} requests to delete their profile", user.Id);

            var result = await _profileService.DeleteProfileAsync(user);

            return result.ResponseType switch
            {
                ResponseType.Forbidden => Forbid(),
                _ => NoContent()
            };
        }
    }
}
