using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.ProfileDto
{
    public class UpdateProfileDto
    {
        [MinLength(2), MaxLength(20)]
        public string? Name { get; set; }
        [Range(1, 100)]
        public int? Age { get; set; }
    }
}
