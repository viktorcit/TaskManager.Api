using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Enums;

namespace TaskManager.Api.Data.DTO
{
    public class BaseResponseDto
    {
        public string ResponseMessage { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = true;
        public ErrorType ErrorType { get; set; }
    }
}
