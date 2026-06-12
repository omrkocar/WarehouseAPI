using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Mappings;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Repositories;

namespace WarehouseAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderRepository orderRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var result = await orderRepository.CreateAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value.ToDto());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order.ToDto());
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await orderRepository.GetAllAsync();
        return Ok(orders.Select(o => o.ToDto()).ToList());
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var result = await orderRepository.UpdateStatusAsync(id, dto.Status);
        if (result.IsSuccess)
            return Ok(result.Value.ToDto());
        
        return BadRequest(result.Error);
    }
}