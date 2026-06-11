using Microsoft.AspNetCore.Mvc;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;
using WarehouseAPI.Repositories;

namespace WarehouseAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductRepository productRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await productRepository.GetAllAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity
        };

        var created = await productRepository.CreateAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await productRepository.DeleteAsync(id);
        if (product == null)
            return NotFound();
        
        return Ok(product);
    }
}