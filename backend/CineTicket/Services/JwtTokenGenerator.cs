using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CineTicket.Models;

namespace CineTicket.Services
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(ApplicationUser user)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                // Định danh người dùng
                new Claim(ClaimTypes.NameIdentifier, user.Id),      // MS
                new Claim("UserId", user.Id),                      // tuỳ biến
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),   // chuẩn JWT

                // Tên/hiển thị
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new Claim("FullName", user.FullName ?? string.Empty),

                // Liên hệ
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty),
                new Claim("Address", user.Address ?? string.Empty),

                // Role
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
                new Claim("role", user.Role ?? string.Empty),

                // jti (id của token)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
