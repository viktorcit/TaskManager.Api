namespace TaskManager.Api.Data.DTO.ResponseDTOs.Profile
{
    public class PublicProfileResponseDto
    {
        public required string Name { get; set; }
        public required string Nickname { get; set; }
        public int? Age { get; set; }
    }
}
