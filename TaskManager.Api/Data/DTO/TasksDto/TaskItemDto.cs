using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Model;

namespace TaskManager.Api.Data.DTO.TasksDto
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
        public Model.TaskStatus Status { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        [Required]
        public bool CanAnyoneJoin { get; set; }
        [Required]
        public string OwnerId { get; set; } = null!;
        [Required]
        public ApplicationUser Owner { get; set; } = null!;
        [Required]
        public string OwnerUsername { get; set; } = null!;
        public List<ApplicationUser> Performers { get; set; } = new();
        public List<string> PerfomersId { get; set; } = new();
    }
}
