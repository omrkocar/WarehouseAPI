using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Common;
using WarehouseAPI.Data;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI;

public class AuthService(WarehouseDbContext dbContext) : IAuthService
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
}