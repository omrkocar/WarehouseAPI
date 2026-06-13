using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Common;
using WarehouseAPI.Data;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Repositories;

public class OrderRepository(WarehouseDbContext dbContext, ILogger<OrderRepository> logger) : IOrderRepository
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
                {
                    logger.LogWarning("Order rejected: product {ProductId} not found.", itemDto.ProductId);
                    return Result<Order>.Failure($"Product {itemDto.ProductId} not found.");
                }

                if (itemDto.Quantity <= 0)
                {
                    logger.LogWarning("Order rejected: invalid quantity {Quantity} for product {ProductId}.",
                        itemDto.Quantity, product.Id);
                    return Result<Order>.Failure($"Quantity for product {product.Id} must be greater than zero.");
                }

                if (product.StockQuantity < itemDto.Quantity)
                {
                    logger.LogWarning("Order rejected: insufficient stock for product {ProductId} ('{ProductName}'). Requested {Requested}, available {Available}.",
                        product.Id, product.Name, itemDto.Quantity, product.StockQuantity);
                    return Result<Order>.Failure(
                        $"Insufficient stock for '{product.Name}'. Requested {itemDto.Quantity}, only {product.StockQuantity} available.");
                }

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
                
                logger.LogInformation("Order {OrderId} created for {CustomerName} with {ItemCount} items, total {Total}",
                    order.Id, order.CustomerName, order.Items.Count, order.Items.Sum(i => i.Quantity * i.PriceAtOrder));
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

        logger.LogWarning("Order rejected: High demand.");
        return Result<Order>.Failure("The order could not be placed due to high demand. Please try again.");
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<Result<Order>> UpdateStatusAsync(Guid id, OrderStatus status)
    {
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        
        if (order == null)
            return Result<Order>.Failure($"Could not find order with id {id}");

        if (!order.CanTransitionTo(status))
            return Result<Order>.Failure($"Illegal operation: Attempting to transition from {order.Status} to {status}");

        if (status == OrderStatus.Cancelled)
            RestockOnCancellation(order);

        order.Status = status;
        await dbContext.SaveChangesAsync();
        return Result<Order>.Success(order);
    }

    // Unlike order placement, this has no optimistic-concurrency retry loop. 
    // Cancellation contention on the same product is rare enough
    // that an occasional conflict is acceptable here.
    private void RestockOnCancellation(Order order)
    {
        foreach (var item in order.Items)
        {
            item.Product.StockQuantity += item.Quantity;
            item.Product.Version = Guid.NewGuid();
        }
    }
}