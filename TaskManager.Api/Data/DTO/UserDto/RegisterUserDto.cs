using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.UserDto
{
    public class RegisterUserDto
    {
        [Required, MaxLength(20)]
        public string? Name { get; set; }
        [Required, MaxLength(20)]
        public string Nickname { get; set; } = null!;
        [Range(1, 100)]
        public int? Age { get; set; }
        [Required, MinLength(6), MaxLength(25)]
        public string Password { get; set; } = null!;

    }
}
