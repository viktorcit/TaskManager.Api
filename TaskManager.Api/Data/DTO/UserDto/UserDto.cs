using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Model;

namespace TaskManager.Api.Data.DTO.UserDto
{
    public class UserDto : IdentityUser
    {
        [MinLength(2), MaxLength(20)]
        public string? Name { get; set; }
        [Required, MinLength(3), MaxLength(20)]
        public string Nickname { get; set; } = null!;
        [Range(1, 100)]
        public int? Age { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<TaskItem>? OwnerTasks { get; set; }
        public List<TaskItem> PerformerTasks { get; set; } = new();
    }
}
