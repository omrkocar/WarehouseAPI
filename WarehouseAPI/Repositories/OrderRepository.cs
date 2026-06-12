using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Repositories;

public class OrderRepository(WarehouseDbContext dbContext) : IOrderRepository
{
    async Task<Order?> IOrderRepository.CreateAsync(CreateOrderDto dto)
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
                return null;
            
            if (product.StockQuantity < itemDto.Quantity)
                return null;
            
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

        return order;
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