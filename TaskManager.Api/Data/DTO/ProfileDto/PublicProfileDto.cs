using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.ProfileDto
{
    public class PublicProfileDto : IdentityUser
    {
        public string? Name { get; set; } = null!;
        public string Nickname { get; set; } = null!;
    }
}
