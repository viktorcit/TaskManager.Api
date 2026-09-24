using TaskManager.Api.Data.DTO;
using TaskManager.Api.Enums;

namespace TaskManager.Api.Helpers
{
    public static class ResponseFactory
    {
        public static BaseResponseDto Ok(string? message)
        {
            return new BaseResponseDto
            {
                IsSuccess = true,
                ResponseMessage = message,
                ResponseType = ResponseType.None
            };
        }
        

        public static BaseResponseDto<T> Ok<T>(T data)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = true,
                ResponseType = ResponseType.None,
                Data = data
            };
        }

        public static BaseResponseDto Fail(ResponseType errorType, string? message = null)
        {
            return new BaseResponseDto
            {
                IsSuccess = false,
                ResponseMessage = message,
                ResponseType = errorType
            };
        }

        public static BaseResponseDto<T> Fail<T>(ResponseType errorType, string? message = null)
        {
            return new BaseResponseDto<T>
            {
                IsSuccess = false,
                ResponseMessage = message,
                ResponseType = errorType,
                Data = default
            };
        }
    }
}
