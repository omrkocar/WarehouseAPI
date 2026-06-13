using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WarehouseAPI.Data;

namespace WarehouseAPI.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    private readonly string dbPath = $"test_{Guid.NewGuid()}.db";
    
    public WarehouseDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        var context = new WarehouseDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        if (File.Exists(dbPath))
            File.Delete(dbPath);
    }
}