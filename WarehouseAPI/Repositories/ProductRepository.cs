using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;
using WarehouseAPI.Models.Entities;

namespace WarehouseAPI.Repositories;

public class ProductRepository(WarehouseDbContext dbContext) : IProductRepository
{
    async Task<List<Product>> IProductRepository.GetAllAsync()
    {
        return await dbContext.Products.ToListAsync();
    }

    async Task<Product?> IProductRepository.GetByIdAsync(Guid id)
    {
        return await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    async Task<Product> IProductRepository.CreateAsync(Product product)
    {
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();
        return product;
    }

    async Task<Product?> IProductRepository.UpdateAsync(Guid id, Product product)
    {
        var existing = await dbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null)
            return null;

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.StockQuantity = product.StockQuantity;

        await dbContext.SaveChangesAsync();
        return existing;
    }

    async Task<Product?> IProductRepository.DeleteAsync(Guid id)
    {
        var existing = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null)
            return null;

        // soft deleting with flag instead of removing. They're then filtered out by the DbContext
        existing.IsDiscontinued = true;
        await dbContext.SaveChangesAsync();
        return existing;
    }
}