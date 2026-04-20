using TaskManager.Api.Enums;

namespace TaskManager.Api.Data.DTO.JoinToTaskDto
{
    public class JoinToTaskRequestSummaryDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public RequestStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
