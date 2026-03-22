using System.ComponentModel.DataAnnotations;
using static TaskManager.Api.Model.JoinToTaskRequest;

namespace TaskManager.Api.Data.DTO.JoinDto
{
    public class JoinToTaskRequestDto
    {
        [Required]
        [MinLength(50), MaxLength(300)]
        public string Description { get; set; } = null!;
    }
}
