using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.RequestDTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required, MinLength(2), MaxLength(25)]
        public required string Name { get; set; }
        [Required, MinLength(2), MaxLength(25)]
        public required string UserName { get; set; }
        [Range(1, 100)]
        public int? Age { get; set; }
        [Required, MinLength(6), MaxLength(25)]
        public required string Password { get; set; }

    }
}
