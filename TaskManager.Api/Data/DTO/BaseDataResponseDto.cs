namespace TaskManager.Api.Data.DTO
{
    public class BaseDataResponseDto<T> : BaseResponseDto
    {
        public T? Data { get; set; }
    }
}
