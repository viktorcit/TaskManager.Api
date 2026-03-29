using Microsoft.AspNetCore.Identity;

namespace TaskManager.Api.Data.DTO.UserDto
{
    public class AdminUpdateUserDto
    {
        public string? Name { get; set; }
        public string Nickname { get; set; } = null!;
        public int? Age { get; set; }
    }
}
