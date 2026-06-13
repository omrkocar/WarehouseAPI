using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WarehouseAPI.Common;
using WarehouseAPI.Data;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI;

public class AuthService(WarehouseDbContext dbContext, IConfiguration configuration) : IAuthService
{
    public async Task<Result<string>> RegisterAsync(RegisterDto dto)
    {
        var existing = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (existing != null)
            return Result<string>.Failure("Username is already taken.");

        var user = new User
        {
            Username =  dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Customer"
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return Result<string>.Success("User successfully registered.");
    }
    
    public async Task<Result<string>> LoginAsync(LoginDto dto)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Result<string>.Failure("Invalid username or password.");

        var token = GenerateToken(user);
        return Result<string>.Success(token);
    }
    
    private string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(configuration["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}