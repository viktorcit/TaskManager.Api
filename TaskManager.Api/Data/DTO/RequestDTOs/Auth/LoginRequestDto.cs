using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.RequestDTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
