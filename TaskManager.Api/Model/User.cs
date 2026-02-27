using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Model
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string Name { get; set; } = null!;
        public int Age { get; set; }
    }
}
