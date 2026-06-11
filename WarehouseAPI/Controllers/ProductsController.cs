using Microsoft.AspNetCore.Mvc;
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
}