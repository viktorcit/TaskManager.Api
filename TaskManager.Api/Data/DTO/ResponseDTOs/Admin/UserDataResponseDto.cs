
using TaskManager.Api.Data.Contracts;

namespace TaskManager.Api.Data.DTO.ResponseDTOs.Admin
{
    public class UserDataResponseDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Nickname { get; set; }

        public int? Age { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? LockoutEnd {get; set; }

        public bool EmailConfirmed { get; set; }

        public List<TaskModelShortInfo> PerformerTasks { get; set; } = [];
        public List<TaskModelShortInfo> OwnerTasks { get; set; } = [];

    }
}
