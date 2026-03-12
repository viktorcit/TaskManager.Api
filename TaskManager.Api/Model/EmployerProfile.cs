using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Model
{
    public class EmployerProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        [Required]
        [MinLength(2), MaxLength(20)]
        public string CompanyName { get; set; } = null!;
        [Required]
        public string Website { get; set; } = null!;
        [Required]
        [MinLength(50), MaxLength(300)]
        public string Description { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        public void UpdateFromRequest(EmployerRequest request)
        {
            CompanyName = request.CompanyName;
            Website = request.Website;
            Description = request.Description;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
