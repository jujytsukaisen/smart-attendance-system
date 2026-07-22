using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SmartAttendance.Infrastructure.Persistence;
using Xunit;

namespace SmartAttendance.IntegrationTests.Common;

public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<string> GetJwtTokenAsync(string email, string password)
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var response = await Client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<WrappedResponse<SmartAttendance.IntegrationTests.Controllers.AuthResponseDto>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return wrapper!.Data!.Token;
    }

    protected async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var token = await GetJwtTokenAsync(email, password);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task ExecuteDbContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(context);
    }

    protected async Task<T> ExecuteDbContextAsync<T>(Func<ApplicationDbContext, Task<T>> action)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await action(context);
    }
}

public class WrappedResponse<T>
{
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
}

public static class HttpResponseMessageExtensions
{
    public static async Task<T?> ReadWrappedResponseAsync<T>(this HttpResponseMessage response)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var wrapper = await response.Content.ReadFromJsonAsync<WrappedResponse<T>>(options);

        return wrapper != null ? wrapper.Data : default;
    }
}
