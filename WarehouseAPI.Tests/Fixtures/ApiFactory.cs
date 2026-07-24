using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WarehouseAPI.Data;

namespace WarehouseAPI.Tests.Fixtures;

public abstract class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        connection.Open();
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<WarehouseDbContext>>();
            services.AddDbContext<WarehouseDbContext>(o => o.UseSqlite(connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        connection.Dispose();
        base.Dispose(disposing);
    }
}