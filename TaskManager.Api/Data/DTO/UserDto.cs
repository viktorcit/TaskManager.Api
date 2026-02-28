using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        [Required, MaxLength(20)]
        public string Nickname { get; set; } = null!;
        [Range(1, 100)]
        public int Age { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string PasswordHash { get; set; } = null!;
    }
}
