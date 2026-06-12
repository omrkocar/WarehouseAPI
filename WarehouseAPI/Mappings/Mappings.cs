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

    public static OrderDto ToDto(this Order order) => new()
    {
        Id = order.Id,
        CustomerName = order.CustomerName,
        Status = order.Status.ToString(),
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(i => i.ToDto()).ToList(),
        Total = order.Items.Sum(i => i.Quantity * i.PriceAtOrder)
    };

    public static OrderItemDto ToDto(this OrderItem item) => new()
    {
        ProductId = item.ProductId,
        Quantity = item.Quantity,
        PriceAtOrder = item.PriceAtOrder
    };
}