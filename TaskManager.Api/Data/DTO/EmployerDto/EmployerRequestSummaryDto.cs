using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Enums;

namespace TaskManager.Api.Data.DTO.EmployerDto
{
    public class EmployerRequestSummaryDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        [Required]
        [MinLength(2), MaxLength(20)]
        public string CompanyName { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public RequestStatus Status { get; set; }
        public string Description { get; set; } = null!;
    }
}
