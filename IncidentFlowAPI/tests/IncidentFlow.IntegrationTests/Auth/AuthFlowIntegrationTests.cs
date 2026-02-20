using System.Net;
using System.Net.Http.Json;
using IncidentFlow.API.Services;
using IncidentFlow.Domain.Entities;
using IncidentFlow.IntegrationTests.Infrastructure;
using IncidentFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentFlow.IntegrationTests.Auth;

public class AuthFlowIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuthFlowIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_SetsAuthAndCsrfCookies_AndReturnsCsrfHeader()
    {
        using var client = CreateClient();
        var credentials = await SeedUserAsync();

        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UsernameOrEmail = credentials.Username,
            Password = credentials.Password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.True(loginResponse.Headers.TryGetValues("X-CSRF-TOKEN", out var csrfHeaders));
        Assert.False(string.IsNullOrWhiteSpace(csrfHeaders?.SingleOrDefault()));

        var setCookieValues = loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.ToList()
            : [];

        Assert.Contains(setCookieValues, cookie => cookie.Contains("incidentflow_access", StringComparison.Ordinal));
        Assert.Contains(setCookieValues, cookie => cookie.Contains("incidentflow_csrf", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logout_WithAuthCookieButMissingCsrfHeader_ReturnsForbidden()
    {
        using var client = CreateClient();
        var credentials = await SeedUserAsync();

        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequest
        {
            UsernameOrEmail = credentials.Username,
            Password = credentials.Password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var logoutResponse = await client.PostAsync("/api/Auth/logout", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, logoutResponse.StatusCode);
        var payload = await logoutResponse.Content.ReadAsStringAsync();
        Assert.Contains("CSRF token validation failed", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Logout_WithoutAuthCookie_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var logoutResponse = await client.PostAsync("/api/Auth/logout", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, logoutResponse.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    private async Task<(string Username, string Password)> SeedUserAsync()
    {
        var username = $"test-user-{Guid.NewGuid():N}";
        var password = "P@ssword123!";

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IncidentFlowDbContext>();
        var hashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

        var user = new User
        {
            Username = username,
            Email = $"{username}@test.local",
            FullName = "Integration Test User",
            Role = "User",
            PasswordHash = hashService.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return (username, password);
    }

    private sealed record LoginRequest
    {
        public string UsernameOrEmail { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
