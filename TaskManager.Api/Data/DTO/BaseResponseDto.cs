using TaskManager.Api.Enums;

namespace TaskManager.Api.Data.DTO
{
    public class BaseResponseDto
    {
        public string ResponseMessage { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = true;
        public ErrorType ErrorType { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
