namespace TaskManager.Api.Data.Contracts
{
    public class AccessTokenInfo
    {
        public required string UserName { get; set; }
        public required string UserId { get; set; }
        public IReadOnlyCollection<string> UserRoles { get; set; } = [];
    }
}
