using Microsoft.Extensions.Logging.Abstractions;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;
using WarehouseAPI.Repositories;
using WarehouseAPI.Tests.Fixtures;

namespace WarehouseAPI.Tests;

public class OrderRepositoryTests : IDisposable
{
    private readonly DatabaseFixture fixture = new();
    
    [Fact]
    public async Task CreateAsync_WithSufficientStock_Succeeds()
    {
        await using var context = fixture.CreateContext();
        var product = new Product { Name = "TestProduct", Price = 10m, StockQuantity = 5, Version = Guid.NewGuid() };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repo = new OrderRepository(context, NullLogger<OrderRepository>.Instance);
        var dto = new CreateOrderDto
        {
            CustomerName = "Test",
            Items = [new CreateOrderItemDto { ProductId = product.Id, Quantity = 3 }]
        };

        var result = await repo.CreateAsync(dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, (await context.Products.FindAsync(product.Id))!.StockQuantity);
    }
    
    public void Dispose() => fixture.Dispose();
}