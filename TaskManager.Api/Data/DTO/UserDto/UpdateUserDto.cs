using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.UserDto
{
    public class UpdateUserDto
    {
        public string Name { get; set; } = null!;
        public int Age { get; set; }
    }
}
