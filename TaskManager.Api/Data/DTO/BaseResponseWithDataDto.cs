namespace TaskManager.Api.Data.DTO
{
    public class BaseResponseWithDataDto<T> : BaseResponseDto
    {
        public T? Data { get; set; }
    }
}
