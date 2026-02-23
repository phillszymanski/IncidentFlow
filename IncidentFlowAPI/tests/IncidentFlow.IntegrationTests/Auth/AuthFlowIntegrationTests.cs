using System.Net;
using System.Net.Http.Json;
using IncidentFlow.API.Contracts.Auth;
using IncidentFlow.IntegrationTests.Infrastructure;

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
        using var client = IntegrationTestData.CreateClient(_factory);
        var (user, password) = await IntegrationTestData.SeedUserAsync(_factory);

        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            UsernameOrEmail = user.Username,
            Password = password
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
        using var client = IntegrationTestData.CreateClient(_factory);
        var (user, password) = await IntegrationTestData.SeedUserAsync(_factory);

        var loginResponse = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            UsernameOrEmail = user.Username,
            Password = password
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
        using var client = IntegrationTestData.CreateClient(_factory);

        var logoutResponse = await client.PostAsync("/api/Auth/logout", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithMissingUsernameOrPassword_ReturnsBadRequest()
    {
        using var client = IntegrationTestData.CreateClient(_factory);

        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            UsernameOrEmail = "",
            Password = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        using var client = IntegrationTestData.CreateClient(_factory);
        var (user, _) = await IntegrationTestData.SeedUserAsync(_factory);

        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            UsernameOrEmail = user.Username,
            Password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutAuthCookie_ReturnsUnauthorized()
    {
        using var client = IntegrationTestData.CreateClient(_factory);

        var response = await client.GetAsync("/api/Auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
