using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.RequestDTOs.Task
{
    public class CreateTaskRequestDto
    {
        [Required, MinLength(2), MaxLength(100)]
        public required string Title { get; set; }
        [Required, MinLength(2), MaxLength(300)]
        public required string Description { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        [Required]
        public required bool CanAnyoneJoin { get; set; }
        [Required]
        public required List<string> Checklist { get; set; }
    }
}
