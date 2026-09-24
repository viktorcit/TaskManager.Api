using TaskManager.Api.Enums;

namespace TaskManager.Api.Entity
{
    public class EmployerRequest
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public RequestStatus Status { get; set; }
        public required string CompanyName { get; set; }
        public required string Website { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
        public string? AdminComment { get; set; }
    }
}
