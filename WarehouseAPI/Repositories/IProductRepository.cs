using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(Guid id, Product product);
    Task<Product?> DeleteAsync(Guid id);
}