using System.Net;
using System.Net.Http.Json;
using IncidentFlow.API.Contracts.Auth;
using IncidentFlow.API.Services;
using IncidentFlow.Domain.Entities;
using IncidentFlow.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace IncidentFlow.IntegrationTests.Infrastructure;

public static class IntegrationTestData
{
    public static HttpClient CreateClient(ApiWebApplicationFactory factory)
    {
        return factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    public static async Task<(User User, string Password)> SeedUserAsync(
        ApiWebApplicationFactory factory,
        string role = "User",
        string? username = null,
        string? password = null)
    {
        var effectiveUsername = username ?? $"test-user-{Guid.NewGuid():N}";
        var effectivePassword = password ?? "P@ssword123!";

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IncidentFlowDbContext>();
        var hashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

        var user = new User
        {
            Username = effectiveUsername,
            Email = $"{effectiveUsername}@test.local",
            FullName = "Integration Test User",
            Role = role,
            PasswordHash = hashService.HashPassword(effectivePassword),
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return (user, effectivePassword);
    }

    public static async Task<Incident> SeedIncidentAsync(
        ApiWebApplicationFactory factory,
        Guid createdBy,
        Guid? assignedTo = null)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IncidentFlowDbContext>();

        var incident = new Incident(
            title: $"Seeded Incident {Guid.NewGuid():N}",
            description: "Seeded incident for integration testing.",
            severity: SeverityLevel.Medium,
            createdBy: createdBy)
        {
            AssignedTo = assignedTo,
            UpdatedAt = DateTime.UtcNow
        };

        await dbContext.Incidents.AddAsync(incident);
        await dbContext.SaveChangesAsync();

        return incident;
    }

    public static async Task<Comment> SeedCommentAsync(
        ApiWebApplicationFactory factory,
        Guid incidentId,
        Guid createdByUserId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IncidentFlowDbContext>();

        var comment = new Comment
        {
            IncidentId = incidentId,
            Content = "Seeded comment",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        await dbContext.Comments.AddAsync(comment);
        await dbContext.SaveChangesAsync();

        return comment;
    }

    public static async Task<IncidentLog> SeedIncidentLogAsync(
        ApiWebApplicationFactory factory,
        Guid incidentId,
        Guid performedByUserId)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IncidentFlowDbContext>();

        var log = new IncidentLog
        {
            IncidentId = incidentId,
            Action = "SeededAction",
            Details = "Seeded details",
            CreatedAt = DateTime.UtcNow,
            PerformedByUserId = performedByUserId
        };

        await dbContext.IncidentLogs.AddAsync(log);
        await dbContext.SaveChangesAsync();

        return log;
    }

    public static async Task<string> LoginAndGetCsrfAsync(HttpClient client, string usernameOrEmail, string password)
    {
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginRequestDto
        {
            UsernameOrEmail = usernameOrEmail,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return response.Headers.GetValues("X-CSRF-TOKEN").Single();
    }

    public static HttpClient CreateAuthenticatedClient(
        ApiWebApplicationFactory factory,
        Guid? userId = null,
        string username = "test-user",
        string email = "test-user@test.local",
        string role = "User",
        params string[] permissions)
    {
        var client = CreateClient(factory);

        var effectiveUserId = userId ?? Guid.NewGuid();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, effectiveUserId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.UsernameHeader, username);
        client.DefaultRequestHeaders.Add(TestAuthHandler.EmailHeader, email);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);

        if (permissions.Length > 0)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, string.Join(',', permissions));
        }

        return client;
    }
}
