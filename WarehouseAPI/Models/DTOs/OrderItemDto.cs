namespace WarehouseAPI.Models.DTOs;

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
}