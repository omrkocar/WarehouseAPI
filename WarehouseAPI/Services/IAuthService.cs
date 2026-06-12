using WarehouseAPI.Common;
using WarehouseAPI.Models.DTOs;

namespace WarehouseAPI;

public interface IAuthService
{
    Task<Result<string>> RegisterAsync(RegisterDto dto);
}