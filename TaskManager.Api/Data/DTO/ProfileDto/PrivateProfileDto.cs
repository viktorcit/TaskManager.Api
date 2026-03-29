using TaskManager.Api.Model;

namespace TaskManager.Api.Data.DTO.ProfileDto
{
    public class PrivateProfileDto
    {
        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string Nickname { get; set; } = null!;
        public int? Age { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<TaskItem>? OwnerTasks { get; set; }
        public List<TaskItem> PerformerTasks { get; set; } = new();
    }
}
