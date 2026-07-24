using System.Net.Http.Json;

namespace WarehouseAPI.Tests.Fixtures;

public static class AuthHelper
{
    public static async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password
        });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    public static async Task RegisterAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            password
        });
        response.EnsureSuccessStatusCode();
    }

    private record LoginResponse(string Token);
}