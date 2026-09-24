using TaskManager.Api.Enums;

namespace TaskManager.Api.Data.DTO.ResponseDTOs.Employer
{
    public class RequestToEmployerRoleDto
    {
        public int RequestId { get; set; }
        public required string UserId { get; set; }
        public required string CompanyName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public RequestStatus Status { get; set; }
        public required string Description { get; set; }
        public required string Website { get; set; }
    }
}
