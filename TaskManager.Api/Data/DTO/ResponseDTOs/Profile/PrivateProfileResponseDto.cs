using TaskManager.Api.Data.Contracts;

namespace TaskManager.Api.Data.DTO.ResponseDTOs.Profile
{
    public class PrivateProfileResponseDto
    {
        public required string Name { get; set; }
        public required string Nickname { get; set; }
        public int? Age { get; set; }
        public List<TaskModelShortInfo> PerformerTasks { get; set; } = [];
        public List<TaskModelShortInfo> OwnerTasks { get; set; } = [];
    }
}
