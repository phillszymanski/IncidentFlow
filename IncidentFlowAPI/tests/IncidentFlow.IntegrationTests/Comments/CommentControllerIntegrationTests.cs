using System.Net;
using System.Net.Http.Json;
using IncidentFlow.API.Contracts.Comments;
using IncidentFlow.IntegrationTests.Infrastructure;

namespace IncidentFlow.IntegrationTests.Comments;

public class CommentControllerIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public CommentControllerIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = IntegrationTestData.CreateClient(_factory);

        var response = await client.GetAsync("/api/Comment");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = IntegrationTestData.CreateClient(_factory);

        var response = await client.PostAsJsonAsync("/api/Comment", new
        {
            incidentId = Guid.NewGuid(),
            content = "Anonymous comment should fail"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenCommentNotOwnedAndUserLacksEditAny_ReturnsForbidden()
    {
        var otherUserId = Guid.NewGuid();
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            userId: otherUserId,
            role: "User",
            permissions: ["comments:create", "comments:edit:own"]);

        var (owner, _) = await IntegrationTestData.SeedUserAsync(_factory, role: "User");
        var incident = await IntegrationTestData.SeedIncidentAsync(_factory, owner.Id);
        var comment = await IntegrationTestData.SeedCommentAsync(_factory, incident.Id, owner.Id);

        var response = await client.PutAsJsonAsync($"/api/Comment/{comment.Id}", new CommentUpdateDto
        {
            Content = "Trying to edit someone else's comment"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidPermissions_ReturnsCreated()
    {
        var creatorId = Guid.NewGuid();
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            userId: creatorId,
            permissions: ["incidents:create", "incidents:read"]);

        var incident = await IntegrationTestData.SeedIncidentAsync(_factory, creatorId);

        var createResponse = await client.PostAsJsonAsync("/api/Comment", new CommentCreateDto
        {
            IncidentId = incident.Id,
            Content = "Integration comment"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

    }

    [Fact]
    public async Task Get_WhenAuthorizedAndCommentMissing_ReturnsNotFound()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            permissions: ["incidents:read"]);

        var response = await client.GetAsync($"/api/Comment/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenAuthorizedAndCommentMissing_ReturnsNotFound()
    {
        using var client = IntegrationTestData.CreateAuthenticatedClient(
            _factory,
            permissions: ["incidents:edit:own"]);

        var response = await client.DeleteAsync($"/api/Comment/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
