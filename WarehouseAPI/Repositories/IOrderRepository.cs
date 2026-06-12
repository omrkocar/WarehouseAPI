using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Repositories;

public interface IOrderRepository
{
    Task<Order?> CreateAsync(CreateOrderDto dto);
    Task<Order?> GetByIdAsync(Guid id);
    Task<List<Order>> GetAllAsync();
}