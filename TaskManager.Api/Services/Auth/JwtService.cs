using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Api.Data.Contracts;
using TaskManager.Api.Interfaces.Auth;

namespace TaskManager.Api.Services.Auth
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly string _secretKey;

        public JwtService(IConfiguration config)
        {
            _config = config;
            _secretKey = _config["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:Key не задан.");
        }

        public string GenerateToken(AccessTokenInfo tokenInfo)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, tokenInfo.UserId.ToString()),
                new (ClaimTypes.Name, tokenInfo.UserName)
            };
            foreach (var role in tokenInfo.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }
    }
}
