using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.RequestDTOs.JoinToTask
{
    public class JoinToTaskRequestDto
    {
        [Required]
        [MinLength(50), MaxLength(300)]
        public required string Description { get; set; }
    }
}
