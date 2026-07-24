using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WarehouseAPI.Models.DTOs;
using WarehouseAPI.Tests.Fixtures;

namespace WarehouseAPI.Tests;

public class ProductsEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task GetProducts_WhenNoAuth_Returns200WithList()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        Assert.NotNull(products);
    }

    [Fact]
    public async Task GetProductById_WhenNotFound_Returns404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithSeededAdmin_ReturnsToken()
    {
        var client = factory.CreateClient();

        var token = await AuthHelper.LoginAsync(client, "admin", "Admin123!");

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task PostProduct_WithNoToken_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductDto
        {
            Name = "Widget",
            Price = 10m,
            StockQuantity = 5
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostProduct_WithCustomerToken_Returns403()
    {
        var client = factory.CreateClient();

        var uniqueName = $"customer-{Guid.NewGuid():N}";
        await AuthHelper.RegisterAsync(client, uniqueName, "Password123!");
        var token = await AuthHelper.LoginAsync(client, uniqueName, "Password123!");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductDto
        {
            Name = "Widget",
            Price = 10m,
            StockQuantity = 5
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostProduct_WithAdminToken_Returns201()
    {
        var client = factory.CreateClient();
        var token = await AuthHelper.LoginAsync(client, "admin", "Admin123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductDto
        {
            Name = $"Widget-{Guid.NewGuid():N}",
            Description = "Test widget",
            Price = 19.99m,
            StockQuantity = 5
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);
        Assert.Equal(5, product!.StockQuantity);
    }
}