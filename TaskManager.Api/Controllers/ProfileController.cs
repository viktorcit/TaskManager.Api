using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Data.DTO.ProfileDto;
using TaskManager.Api.Enums;
using TaskManager.Api.Extensions;
using TaskManager.Api.Model;
using TaskManager.Api.Services.Interfaces;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }



        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublicProfileDto>>> GetAllProfilesAsync()
        {
            var profiles = await _profileService.GetAllProfilesAsync(1, 20);

            return profiles;
        }

        [Authorize]
        [HttpGet("{nickname}")]
        public async Task<ActionResult<PublicProfileDto>> GetProfileOfUserAsync(string nickname)
        {
            var user = await _profileService.GetProfileOfUserAsync(nickname);
            if (user == null)
                return NotFound("User not found");

            return user;
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<ActionResult<PrivateProfileDto>> GetProfileAsync()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _profileService.GetProfileAsync(userId);
            if (result == null)
                return Unauthorized("Profile not found");

            return result;
        }


        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteProfileAsync()
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _profileService.DeleteProfileAsync(userId);

            return result.ErrorType switch
            {
                ErrorType.Unauthorized => Unauthorized(result.ResponseMessage),
                ErrorType.Forbidden => Forbid(result.ResponseMessage),
                _ => NoContent()
            };
        }


        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> UpdateProfileAsync(UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _profileService.UpdateProfileAsync(dto, userId);

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
