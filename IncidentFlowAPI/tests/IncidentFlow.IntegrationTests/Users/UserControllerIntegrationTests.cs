using System.Net;
using System.Net.Http.Json;
using IncidentFlow.API.Contracts.Users;
using IncidentFlow.IntegrationTests.Infrastructure;

namespace IncidentFlow.IntegrationTests.Users;

public class UserControllerIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public UserControllerIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_WhenAnonymousAndRoleRequestedAsAdmin_ForcesRoleToUser()
    {
        using var client = IntegrationTestData.CreateClient(_factory);
        var username = $"user-{Guid.NewGuid():N}";

        var response = await client.PostAsJsonAsync("/api/User", new UserCreateDto
        {
            Username = username,
            Email = $"{username}@test.local",
            FullName = "Test User",
            Role = "Admin",
            Password = "P@ssword123!"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.NotNull(payload);
        Assert.Equal("User", payload!.Role);
    }

    [Fact]
    public async Task Create_WhenPasswordMissing_ReturnsBadRequest()
    {
        using var client = IntegrationTestData.CreateClient(_factory);

        var response = await client.PostAsJsonAsync("/api/User", new UserCreateDto
        {
            Username = $"user-{Guid.NewGuid():N}",
            Email = "missing-password@test.local",
            FullName = "Missing Password",
            Password = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = IntegrationTestData.CreateClient(_factory);

        var response = await client.GetAsync("/api/User");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenCallerCanManageUsers_PreservesRequestedRole()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            permissions: ["users:manage"]);

        var username = $"managed-{Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("/api/User", new UserCreateDto
        {
            Username = username,
            Email = $"{username}@test.local",
            FullName = "Managed User",
            Role = "Admin",
            Password = "P@ssword123!"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<UserResponseDto>();
        Assert.NotNull(payload);
        Assert.Equal("Admin", payload!.Role);
    }

    [Fact]
    public async Task GetAll_WhenAuthorizedWithManageUsers_ReturnsOk()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            permissions: ["users:manage"]);

        await IntegrationTestData.SeedUserAsync(_factory);

        var response = await client.GetAsync("/api/User");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenManagedUserMissing_ReturnsNotFound()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            permissions: ["users:manage"]);

        var response = await client.GetAsync($"/api/User/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
