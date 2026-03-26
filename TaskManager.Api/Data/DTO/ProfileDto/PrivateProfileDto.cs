using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Model;

namespace TaskManager.Api.Data.DTO.ProfileDto
{
    public class PrivateProfileDto : IdentityUser
    {
        public string? Name { get; set; }
        public string Nickname { get; set; } = null!;
        public int? Age { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<TaskItem>? OwnerTasks { get; set; }
        public List<TaskItem> PerformerTasks { get; set; } = new();
    }
}
