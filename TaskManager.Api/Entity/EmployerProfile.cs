namespace TaskManager.Api.Entity
{
    public class EmployerProfile
    {
        public int Id { get; set; }
        public required string EmployerId { get; set; }
        public required string CompanyName { get; set; }
        public required string Website { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
