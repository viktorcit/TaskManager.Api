using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.UserDto
{
    public class UserDto : IdentityUser
    {
        [MinLength(2), MaxLength(20)]
        public string? Name { get; set; }
        [Required, MaxLength(20)]
        public string Nickname { get; set; } = null!;
        [Range(1, 100)]
        public int? Age { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
