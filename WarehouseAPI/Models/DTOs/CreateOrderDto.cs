namespace WarehouseAPI.Models.DTOs;

public class CreateOrderDto
{
    public required string CustomerName { get; set; }
    public required List<CreateOrderItemDto> Items { get; set; }
}