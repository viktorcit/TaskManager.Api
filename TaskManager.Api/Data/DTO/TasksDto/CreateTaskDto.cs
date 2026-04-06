using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Model;

namespace TaskManager.Api.Data.DTO.TasksDto
{
    public class CreateTaskDto
    {
        [Required, MaxLength(100)]
        public string Title { get; set; } = null!;
        [Required, MaxLength(300)]
        public string? Description { get; set; }
        [Required]
        public DateTimeOffset? DueDate { get; set; }
        [Required]
        public bool? CanAnyoneJoin { get; set; }
        public List<string> PerformersId { get; set; } = new();
        [Required]
        public List<string> Checklist { get; set; } = new();
    }
}
