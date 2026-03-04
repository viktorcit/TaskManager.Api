using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Model;

namespace TaskManager.Api.Data.DTO
{
    public class TaskItemDto
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; } = null!;
        [MaxLength(300)]
        public string? Description { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTimeOffset? CompletedAt { get; set; }
        [Required]
        public int OwnerId { get; set; }
        public ApplicationUser Owner { get; set; } = null!;
        public List<ApplicationUser> Performers { get; set; } = new();
    }
}
