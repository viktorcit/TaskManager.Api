using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.EmployerDto
{
    public class RequestEmployerDto
    {
        [Required]
        [MinLength(2), MaxLength(20)]
        public string CompanyName { get; set; } = null!;
        [Required]
        public string Website { get; set; } = null!;
        [Required]
        [MinLength(50), MaxLength(300)]
        public string Description { get; set; } = null!;
    }
}
