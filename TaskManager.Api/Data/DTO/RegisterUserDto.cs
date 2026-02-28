namespace TaskManager.Api.Data.DTO
{
    public class RegisterUserDto
    {
        public string? Name { get; set; }
        public string Nickname { get; set; } = null!;
        public int Age { get; set; }
        public string Password { get; set; } = null!;

    }
}
