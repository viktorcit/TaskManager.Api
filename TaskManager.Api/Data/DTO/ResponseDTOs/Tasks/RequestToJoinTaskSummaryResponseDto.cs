namespace TaskManager.Api.Data.DTO.ResponseDTOs.Tasks
{
    public class RequestToJoinTaskSummaryResponseDto
    {
        public required int RequestId { get; set; }
        public required string UserName { get; set; }
    }
}
