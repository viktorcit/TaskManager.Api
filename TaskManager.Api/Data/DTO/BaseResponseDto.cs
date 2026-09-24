using TaskManager.Api.Enums;

namespace TaskManager.Api.Data.DTO
{
    public class BaseResponseDto
    {
        public string? ResponseMessage { get; set; }
        public bool IsSuccess { get; set; }
        public ResponseType ResponseType { get; set; }
    }
    public class BaseResponseDto<T> : BaseResponseDto
    {
        public T? Data { get; set; }
    }

}
