using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.RequestDto.Employer
{
    public class CreateEmployerRoleRequestDto
    {
        [Required]
        [MinLength(2), MaxLength(20)]
        public required string CompanyName { get; set; }
        [Required]
        public required string Website { get; set; }
        [Required]
        [MinLength(50), MaxLength(300)]
        public required string Description { get; set; }
    }
}
