using TaskManager.Api.Data.DTO.UserDto;

namespace TaskManager.Api.Data.DTO.TasksDto
{
    public class TaskItemResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public Enums.TaskStatus Status { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public bool CanAnyoneJoin { get; set; }
        public string OwnerId { get; set; } = null!;
        public string OwnerUsername { get; set; } = null!;
        public List<UserShortDto> Performers { get; set; } = new();
        public List<string> Checklist { get; set; } = new();
    }
}
