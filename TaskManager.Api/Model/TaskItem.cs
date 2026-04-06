using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskManager.Api.Enums.TaskStatus;

namespace TaskManager.Api.Model
{
    public class TaskItem
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; } = null!;
        [MaxLength(300)]
        public string? Description { get; set; }
        [Required]
        public List<string> Checklist { get; set; } = new();
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.InProgress;
        public DateTimeOffset? CompletedAt { get; set; }
        [Required]
        public bool? CanAnyoneJoin { get; set; }
        [Required]
        public string OwnerId { get; set; } = null!;
        [Required]
        public ApplicationUser Owner { get; set; } = null!;
        [Required]
        public string OwnerUsername { get; set; } = null!;
        public List<ApplicationUser> Performers { get; set; } = new();
    }
}
