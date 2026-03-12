using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Api.Model;

namespace TaskManager.Api.JWT
{
    public class JwtService
    {
        private readonly IConfiguration _config;


        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(List<Claim> claims)
        {
            var secretKey = _config["Jwt:Key"];

            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("Jwt:Key не задан.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
