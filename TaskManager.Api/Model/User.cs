using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Model
{
    public class User
    {
        public int Id { get; set; }
        [MinLength(2), MaxLength(20)]
        public string? Name { get; set; }
        [Required,MinLength(3), MaxLength(20)]
        public string Nickname { get; set; } = null!;
        [Range(1, 100)]
        public int? Age { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        [Required, MinLength(6), MaxLength(25)]
        public string PasswordHash { get; set; } = null!;
    }
}
