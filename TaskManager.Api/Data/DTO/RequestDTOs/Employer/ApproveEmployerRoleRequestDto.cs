using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.RequestDTOs.Employer
{
    public class ApproveEmployerRoleRequestDto
    {
        [Required]
        [MinLength(50), MaxLength(300)]
        public required string AdminComment { get; set; }
    }
}
