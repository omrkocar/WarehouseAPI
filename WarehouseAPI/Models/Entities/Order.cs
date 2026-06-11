namespace WarehouseAPI.Models.Entities;

public class Order
{
    public Guid Id { get; set; }
    public required string CustomerName  { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}