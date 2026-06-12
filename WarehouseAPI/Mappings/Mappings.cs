using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Mappings;

public static class Mappings
{
    public static ProductDto ToDto(this Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity
    };
}