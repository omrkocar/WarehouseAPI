namespace WarehouseAPI.Models.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public required string CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }
    public required List<OrderItemDto> Items { get; set; }
}