namespace TaskManager.Api.Data.DTO.TasksDto
{
    public class TaskItemSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string OwnerId { get; set; } = null!;
        public string OwnerUsername { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string Description { get; set; } = null!;
        public Enums.TaskStatus Status { get; set; }
    }
}
