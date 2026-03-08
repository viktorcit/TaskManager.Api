using Microsoft.Extensions.Primitives;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace TaskManager.Api.Model
{
    public class EmployerRequest
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
        public string Status { get; set; } = null!;
        [Required]
        [MinLength(2), MaxLength(20)]
        public string CompanyName { get; set; } = null!;
        [Required]
        public string Website { get; set; } = null!;
        [Required]
        [MinLength(50), MaxLength(300)]
        public string Description { get; set; } = null!; 
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? ReviewedBy { get; set; } = null!;
        public DateTimeOffset? ReviewedAt { get; set; }
        public string AdminComment { get; set; } = null!;
    }
}
