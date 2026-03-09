using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.UserDto
{
    public class LoginUserDto
    {
        [Required, MaxLength(20)]
        public string Nickname { get; set; } = null!;
        [MinLength(6), MaxLength(25)]
        public string Password { get; set; } = null!;
    }
}
