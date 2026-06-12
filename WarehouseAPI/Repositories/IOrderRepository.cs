using WarehouseAPI.Common;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Repositories;

public interface IOrderRepository
{
    Task<Result<Order>> CreateAsync(CreateOrderDto dto);
    Task<Order?> GetByIdAsync(Guid id);
    Task<List<Order>> GetAllAsync();
    Task<Result<Order>> UpdateStatusAsync(Guid id, OrderStatus status);
}