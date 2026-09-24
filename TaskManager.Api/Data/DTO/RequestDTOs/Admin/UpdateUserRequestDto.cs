using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Data.DTO.RequestDTOs.Admin
{
    public class UpdateUserRequestDto
    {
        [MinLength(2), MaxLength(25)]
        public string? Name { get; set; }
        [MinLength(2), MaxLength(25)]
        public string? UserName { get; set; }
        [Range(1, 100)]
        public int? Age { get; set; }
    }
}
