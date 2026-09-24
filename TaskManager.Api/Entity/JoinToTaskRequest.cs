using TaskManager.Api.Enums;

namespace TaskManager.Api.Entity
{
    public class JoinToTaskRequest
    {
        public int Id { get; set; }
        public required int TaskId { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required RequestStatus Status { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ReviewedAt { get; set; }
    }
}
