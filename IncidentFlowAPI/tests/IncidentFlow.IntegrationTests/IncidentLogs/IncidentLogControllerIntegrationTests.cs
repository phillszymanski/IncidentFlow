using System.Net;
using IncidentFlow.IntegrationTests.Infrastructure;

namespace IncidentFlow.IntegrationTests.IncidentLogs;

public class IncidentLogControllerIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public IncidentLogControllerIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = IntegrationTestData.CreateClient(_factory);

        var response = await client.GetAsync("/api/IncidentLog");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByIncident_WhenAuthenticatedWithoutAuditPermission_ReturnsForbidden()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            role: "User",
            permissions: ["incidents:read"]);

        var response = await client.GetAsync($"/api/IncidentLog/incident/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenAdminRequestsMissingLog_ReturnsNotFound()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            role: "Admin",
            permissions: ["incidents:audit:read"]);

        var response = await client.GetAsync($"/api/IncidentLog/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WhenAuthorizedWithAuditPermission_ReturnsOk()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            permissions: ["incidents:audit:read"]);

        var actorId = Guid.NewGuid();
        var incident = await IntegrationTestData.SeedIncidentAsync(_factory, actorId);
        await IntegrationTestData.SeedIncidentLogAsync(_factory, incident.Id, actorId);

        var response = await client.GetAsync("/api/IncidentLog");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByIncident_WhenAuthorizedWithAuditPermission_ReturnsOk()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            permissions: ["incidents:audit:read"]);

        var actorId = Guid.NewGuid();
        var incident = await IntegrationTestData.SeedIncidentAsync(_factory, actorId);
        await IntegrationTestData.SeedIncidentLogAsync(_factory, incident.Id, actorId);

        var response = await client.GetAsync($"/api/IncidentLog/incident/{incident.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
