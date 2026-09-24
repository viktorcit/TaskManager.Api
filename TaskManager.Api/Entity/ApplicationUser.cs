using Microsoft.AspNetCore.Identity;

namespace TaskManager.Api.Entity
{
    public class ApplicationUser : IdentityUser
    {
        public required string Name { get; set; }
        public int? Age { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
