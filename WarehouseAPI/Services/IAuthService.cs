using WarehouseAPI.Common;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI;

public interface IAuthService
{
    Task<Result<string>> RegisterAsync(RegisterDto dto);
    Task<Result<string>> LoginAsync(LoginDto dto);
}