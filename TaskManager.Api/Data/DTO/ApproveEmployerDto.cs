using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO
{
    public class ApproveEmployerDto
    {
        [Required]
        [MinLength(50), MaxLength(300)]
        public string AdminComment { get; set; } = null!;
    }
}
