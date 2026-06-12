using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Common;
using WarehouseAPI.Data;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Repositories;

public class OrderRepository(WarehouseDbContext dbContext) : IOrderRepository
{
public async Task<Result<Order>> CreateAsync(CreateOrderDto dto)
{
    const int maxRetries = 3;

    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        var order = new Order
        {
            CustomerName = dto.CustomerName,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        foreach (var itemDto in dto.Items)
        {
            var product = await dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);

            if (product == null)
                return Result<Order>.Failure($"Product {itemDto.ProductId} not found.");

            if (itemDto.Quantity <= 0)
                return Result<Order>.Failure($"Quantity for product {product.Id} must be greater than zero.");

            if (product.StockQuantity < itemDto.Quantity)
                return Result<Order>.Failure(
                    $"Insufficient stock for '{product.Name}'. Requested {itemDto.Quantity}, only {product.StockQuantity} available.");

            product.StockQuantity -= itemDto.Quantity;
            product.Version = Guid.NewGuid();

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                PriceAtOrder = product.Price
            });
        }

        dbContext.Orders.Add(order);

        try
        {
            await dbContext.SaveChangesAsync();
            return Result<Order>.Success(order);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request changed a product mid-flight
            // So we clear tracked changes and retry with fresh data
            dbContext.ChangeTracker.Clear();
            
            // foreach (var entry in dbContext.ChangeTracker.Entries().ToList())
            //     await entry.ReloadAsync();
        }
    }

    return Result<Order>.Failure("The order could not be placed due to high demand. Please try again.");
}

    async Task<Order?> IOrderRepository.GetByIdAsync(Guid id)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    async Task<List<Order>> IOrderRepository.GetAllAsync()
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .ToListAsync();;
    }
}