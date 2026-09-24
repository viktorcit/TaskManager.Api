
namespace TaskManager.Api.Data.DTO.ResponseDTOs.Tasks
{
    public class RequestToJoinTaskResponseDto
    {
        public required int RequestId { get; set; }
        public required string UserName { get; set; }
        public required string UserId { get; set; }
        public required string Description { get; set; } 
    }
}
