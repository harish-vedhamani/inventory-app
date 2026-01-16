using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Playground.Data;
using Playground.DTOs;
using Playground.Entities;

namespace Playground.Services
{
    public class EfAuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _cfg;

        public EfAuthService(ApplicationDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public async Task RegisterAsync(AuthRegisterDto dto)
        {
            var exists = await _db.Users.AnyAsync(u => u.Username == dto.Username);
            if (exists) throw new ArgumentException("Username already exists");

            var user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> LoginAsync(AuthLoginDto dto)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) throw new UnauthorizedAccessException("Invalid credentials");

            var ok = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!ok) throw new UnauthorizedAccessException("Invalid credentials");

            var token = GenerateToken(user);
            return new AuthResponseDto { Token = token, Username = user.Username, Role = user.Role };
        }

        private string GenerateToken(UserEntity user)
        {
            var key = _cfg.GetValue<string>("Jwt:Key") ?? throw new InvalidOperationException("Jwt:Key missing");
            var issuer = _cfg.GetValue<string>("Jwt:Issuer") ?? "playground";

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
