using TaskManager.Api.Data.DTO.TasksDto;

namespace TaskManager.Api.Data.DTO.ProfileDto
{
    public class PrivateProfileDto
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string Nickname { get; set; } = null!;
        public int? Age { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<TaskItemShortDto>? OwnerTasks { get; set; }
        public List<TaskItemShortDto> PerformerTasks { get; set; } = new();
    }
}
