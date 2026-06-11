namespace WarehouseAPI.Models.Entities;

public class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity  { get; set; }
    public bool IsDiscontinued { get; set; }
}