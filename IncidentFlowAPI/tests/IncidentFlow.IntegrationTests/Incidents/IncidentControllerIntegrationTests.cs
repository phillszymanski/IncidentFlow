using System.Net;
using System.Net.Http.Json;
using IncidentFlow.API.Contracts.Incidents;
using IncidentFlow.IntegrationTests.Infrastructure;

namespace IncidentFlow.IntegrationTests.Incidents;

public class IncidentControllerIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public IncidentControllerIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = IntegrationTestData.CreateClient(_factory);

        var response = await client.GetAsync("/api/Incident");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithInvalidPagingAndFilter_NormalizesInputs()
    {
        var userId = Guid.NewGuid();
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            userId: userId,
            role: "User",
            permissions: ["incidents:read"]);

        await IntegrationTestData.SeedIncidentAsync(_factory, userId);

        var response = await client.GetAsync("/api/Incident?page=0&pageSize=999&filter=does-not-exist");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<PagedIncidentResponseDto>();
        Assert.NotNull(payload);
        Assert.Equal(1, payload!.Page);
        Assert.Equal(50, payload.PageSize);
    }

    [Fact]
    public async Task Create_WhenUserLacksAssignPermission_AndAssignedToProvided_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            userId: userId,
            role: "User",
            permissions: ["incidents:create"]);

        var (assignee, _) = await IntegrationTestData.SeedUserAsync(_factory, role: "User");

        var response = await client.PostAsJsonAsync("/api/Incident", new IncidentCreateDto
        {
            Title = "Permission edge case",
            Description = "Attempt assignment without assign permission",
            Severity = SeverityLevel.High,
            CreatedBy = userId,
            AssignedTo = assignee.Id
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenIncidentMissing_ReturnsNotFound()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            permissions: ["incidents:read"]);

        var response = await client.GetAsync($"/api/Incident/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_StatusOnly_WithLimitedPermissionOnOwnedIncident_ReturnsNoContent()
    {
        var ownerId = Guid.NewGuid();
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            userId: ownerId,
            permissions: ["incidents:read", "incidents:status:limited"]);

        var incident = await IntegrationTestData.SeedIncidentAsync(_factory, ownerId);

        var response = await client.PutAsJsonAsync($"/api/Incident/{incident.Id}", new IncidentUpdateDto
        {
            Status = IncidentStatus.InProgress
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAndRestore_WithProperPermissions_ReturnNoContent()
    {
        var actorId = Guid.NewGuid();
        using var deleteClient = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            userId: actorId,
            permissions: ["incidents:delete"]);
        using var restoreClient = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            userId: actorId,
            permissions: ["incidents:restore"]);

        var incident = await IntegrationTestData.SeedIncidentAsync(_factory, actorId);

        var deleteResponse = await deleteClient.DeleteAsync($"/api/Incident/{incident.Id}");
        var restoreResponse = await restoreClient.PostAsync($"/api/Incident/{incident.Id}/restore", null);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, restoreResponse.StatusCode);
    }

    [Fact]
    public async Task GetDashboardSummary_WithReadPermission_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            userId: userId,
            permissions: ["incidents:read"]);

        await IntegrationTestData.SeedIncidentAsync(_factory, userId);

        var response = await client.GetAsync("/api/Incident/dashboard-summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
