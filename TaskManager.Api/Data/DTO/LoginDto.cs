using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO
{
    public class LoginDto
    {
        [Required, MaxLength(20)]
        public string Nickname { get; set; } = null!;
        [Range(1, 100)]
        public string Password { get; set; } = null!;
    }
}
