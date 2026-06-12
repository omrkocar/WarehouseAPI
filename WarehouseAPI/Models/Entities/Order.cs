namespace WarehouseAPI.Models.Entities;

public class Order
{
    public Guid Id { get; set; }
    public required string CustomerName  { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    
    public bool CanTransitionTo(OrderStatus next)
    {
        return Status switch
        {
            OrderStatus.Pending => next is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => next is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => next is OrderStatus.Delivered, // cannot cancel order once shipped
            OrderStatus.Delivered => false, // terminal 
            OrderStatus.Cancelled => false, // terminal
            _ => false
        };
    }
}