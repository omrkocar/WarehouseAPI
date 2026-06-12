using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Common;
using WarehouseAPI.Data;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Repositories;

public class OrderRepository(WarehouseDbContext dbContext) : IOrderRepository
{
    async Task<Result<Order>> IOrderRepository.CreateAsync(CreateOrderDto dto)
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
                return Result<Order>.Failure($"Product {itemDto.ProductId} not found");
            
            if (itemDto.Quantity <= 0)
                return Result<Order>.Failure($"Quantity for product {product.Id} must be greater than zero.");

            if (product.StockQuantity < itemDto.Quantity)
                return Result<Order>.Failure(
                    $"Insufficient stock for '{product.Name}'. Requested {itemDto.Quantity}, available {product.StockQuantity}.");
            
            product.StockQuantity -= itemDto.Quantity;
            
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                PriceAtOrder = product.Price
            });
        }
        
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        return Result<Order>.Success(order);
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