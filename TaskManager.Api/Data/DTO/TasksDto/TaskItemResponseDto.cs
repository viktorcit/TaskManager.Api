using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Model;

namespace TaskManager.Api.Data.DTO.TasksDto
{
    public class TaskItemResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public Model.TaskStatus Status { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public bool CanAnyoneJoin { get; set; }
        public string OwnerId { get; set; } = null!;
        public ApplicationUser Owner { get; set; } = null!;
        public List<ApplicationUser> Performers { get; set; } = new();
    }
}
