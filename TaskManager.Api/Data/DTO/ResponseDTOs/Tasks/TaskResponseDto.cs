using TaskManager.Api.Data.Contracts;

namespace TaskManager.Api.Data.DTO.ResponseDTOs.Tasks
{
    public class TaskResponseDto
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public required Enums.TaskStatus Status { get; set; }
        public required string OwnerUsername { get; set; }
        public List<UserShortInfo> Performers { get; set; } = [];
        public List<string> Checklist { get; set; } = [];
        public required bool CanAnyoneJoin { get; set; }
    }
}
