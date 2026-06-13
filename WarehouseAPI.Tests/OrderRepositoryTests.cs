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
    
    [Fact]
    public async Task CreateAsync_WithInsufficientStock_ReturnsFailure()
    {
        await using var context = fixture.CreateContext();
        var product = new Product { Name = "TestProduct", Price = 10m, StockQuantity = 2, Version = Guid.NewGuid() };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repo = new OrderRepository(context, NullLogger<OrderRepository>.Instance);
        var dto = new CreateOrderDto
        {
            CustomerName = "Test",
            Items = [new CreateOrderItemDto { ProductId = product.Id, Quantity = 5 }]
        };

        var result = await repo.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("Insufficient stock", result.Error);
        Assert.Equal(2, (await context.Products.FindAsync(product.Id))!.StockQuantity); // quantity must be unchanged
    }
    
    [Fact]
    public async Task CreateAsync_WithZeroQuantity_ReturnsFailure()
    {
        await using var context = fixture.CreateContext();
        var product = new Product { Name = "Test Product", Price = 10m, StockQuantity = 5, Version = Guid.NewGuid() };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repo = new OrderRepository(context, NullLogger<OrderRepository>.Instance);
        var dto = new CreateOrderDto
        {
            CustomerName = "Test",
            Items = [new CreateOrderItemDto { ProductId = product.Id, Quantity = 0 }]
        };

        var result = await repo.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("greater than zero", result.Error);
    }
    
    [Fact]
    public async Task CreateAsync_WithNonexistentProduct_ReturnsFailure()
    {
        await using var context = fixture.CreateContext();
        var repo = new OrderRepository(context, NullLogger<OrderRepository>.Instance);
        var dto = new CreateOrderDto
        {
            CustomerName = "Test",
            Items = [new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 }]
        };

        var result = await repo.CreateAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task CreateAsync_ConcurrentOrdersForLastUnit_OnlyOneSucceeds()
    {
        var productId = Guid.NewGuid();
        using (var context = fixture.CreateContext())
        {
            var product = new Product { Id = productId, Name = "Test Product", Price = 10m, StockQuantity = 1, Version = Guid.NewGuid() };
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        var dto = new CreateOrderDto
        {
            CustomerName = "Test",
            Items = [new CreateOrderItemDto { ProductId = productId, Quantity = 1 }]
        };

        using var contextA = fixture.CreateContext();
        using var contextB = fixture.CreateContext();
        var repoA = new OrderRepository(contextA, NullLogger<OrderRepository>.Instance);
        var repoB = new OrderRepository(contextB, NullLogger<OrderRepository>.Instance);
        
        var taskA = repoA.CreateAsync(dto);
        var taskB = repoB.CreateAsync(dto);
        
        var results = await Task.WhenAll(taskA, taskB);
        var successCount = results.Count(r => r.IsSuccess);
        Assert.Equal(1, successCount);
        
        using var verify = fixture.CreateContext();
        var finalStock = (await verify.Products.FindAsync(productId))!.StockQuantity;
        Assert.Equal(0, finalStock);
    }
    
    public void Dispose() => fixture.Dispose();
}