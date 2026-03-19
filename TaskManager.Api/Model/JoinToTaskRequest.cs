using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Model
{
    public class JoinToTaskRequest
    {
        public int Id { get; set; }
        [Required]
        public int TaskId { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public JoinRequestStatus Status { get; set; }
        [Required]
        [MinLength(50), MaxLength(300)]
        public string Description { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ReviwedAt { get; set; }


        public enum JoinRequestStatus
        {
            Pending,
            Approved,
            Rejected
        }
    }
}
