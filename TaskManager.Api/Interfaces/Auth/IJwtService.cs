using TaskManager.Api.Data.Contracts;

namespace TaskManager.Api.Interfaces.Auth
{
    public interface IJwtService
    {
        string GenerateToken(AccessTokenInfo tokenInfo);
    }
}
