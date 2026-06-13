using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Models.DTOs;

namespace WarehouseAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);

        if (!result.IsSuccess)
            return Unauthorized(result.Error);

        return Ok(new { token = result.Value });
    }
}