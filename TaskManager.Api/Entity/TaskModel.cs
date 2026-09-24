using TaskStatus = TaskManager.Api.Enums.TaskStatus;

namespace TaskManager.Api.Entity
{
    public class TaskModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public List<string> Checklist { get; set; } = [];
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.InProgress;
        public required bool CanAnyoneJoin { get; set; }
        public required string OwnerId { get; set; }
        public required string OwnerUserName { get; set; }
    }
}
