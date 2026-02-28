using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Model
{
    public class TaskItem
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
        public User Owner { get; set; } = null!;
        public List<User> Performers { get; set; } = new();
    }
}
