using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.RequestDto.EmployerDto
{
    public class RejectEmployerRoleRequestDto
    {
        [Required]
        [MinLength(50), MaxLength(300)]
        public required string Reason { get; set; }
    }
}
