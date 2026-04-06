using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.EmployerDto
{
    public class RejectEmployerDto
    {
        [Required]
        [MinLength(50), MaxLength(300)]
        public string reason { get; set; } = null!;
    }
}
