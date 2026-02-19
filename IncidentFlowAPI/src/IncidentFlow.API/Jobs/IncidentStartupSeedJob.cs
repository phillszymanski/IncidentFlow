using IncidentFlow.Domain.Entities;
using IncidentFlow.API.Services;
using IncidentFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IncidentFlow.API.Jobs;

public sealed class IncidentStartupSeedJob : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IncidentStartupSeedJob> _logger;
    private readonly IncidentSeedOptions _seedOptions;
    private const string AdminEmail = "admin@test.com";
    private const string AdminPassword = "adminpass";
    private const string AdminUsername = "admin";
    private static readonly string[] CommentTemplates =
    [
        "Investigating root cause.",
        "Escalated to on-call engineer.",
        "Monitoring after mitigation.",
        "Additional diagnostic data collected.",
        "Customer impact confirmed and communicated.",
        "Awaiting validation from affected team."
    ];

    private static readonly string[] LogActionTemplates =
    [
        "Status change",
        "Assignment change",
        "Severity change",
        "Comment added",
        "Incident update"
    ];

    private static readonly IncidentStatus[] SeedStatuses =
    [
        IncidentStatus.Open,
        IncidentStatus.InProgress,
        IncidentStatus.Resolved,
        IncidentStatus.Closed
    ];

    private static readonly SeverityLevel[] SeedSeverities =
    [
        SeverityLevel.Low,
        SeverityLevel.Medium,
        SeverityLevel.High,
        SeverityLevel.Critical
    ];

    public IncidentStartupSeedJob(
        IServiceProvider serviceProvider,
        ILogger<IncidentStartupSeedJob> logger,
        IOptions<IncidentSeedOptions> seedOptions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _seedOptions = seedOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_seedOptions.Enabled)
        {
            _logger.LogInformation("Incident startup seed is disabled by configuration.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IncidentFlowDbContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        var adminUser = await dbContext.Users
            .FirstOrDefaultAsync(user => user.Email.ToLower() == AdminEmail.ToLower(), cancellationToken);

        if (adminUser is null)
        {
            adminUser = new User
            {
                Username = AdminUsername,
                FullName = "System Administrator",
                Email = AdminEmail,
                Role = "Admin",
                PasswordHash = passwordHashService.HashPassword(AdminPassword),
                CreatedAt = DateTime.UtcNow
            };

            await dbContext.Users.AddAsync(adminUser, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Seeded default admin user {AdminEmail}.", AdminEmail);
        }
        else
        {
            _logger.LogInformation("Admin user {AdminEmail} already exists. Skipping admin seed.", AdminEmail);
        }

        var actingUserId = await ResolveSeedUserIdAsync(dbContext, adminUser.Id, cancellationToken);

        var seedItems = _seedOptions.Items
            .Where(item => !string.IsNullOrWhiteSpace(item.Title))
            .DistinctBy(item => new
            {
                Title = item.Title.Trim(),
                Description = item.Description.Trim(),
                item.Severity
            })
            .ToList();

        if (seedItems.Count == 0)
        {
            _logger.LogInformation("No incident seed items configured. Skipping startup seed.");
            return;
        }

        var incidentCount = Math.Max(0, _seedOptions.NumberOfIncidents);
        var logsPerIncident = Math.Max(0, _seedOptions.IncidentLogsPerIncident);

        if (incidentCount == 0)
        {
            _logger.LogInformation("Incident seed count is set to 0. Skipping startup seed.");
            return;
        }

        var existingIncidentIds = await dbContext.Incidents
            .AsNoTracking()
            .Where(incident => !incident.IsDeleted)
            .Select(incident => incident.Id)
            .ToListAsync(cancellationToken);

        var existingIncidentCount = existingIncidentIds.Count;
        var incidentsToCreate = Math.Max(0, incidentCount - existingIncidentCount);

        var random = new Random();
        var incidents = new List<Incident>(incidentsToCreate);

        for (var index = 0; index < incidentsToCreate; index++)
        {
            var template = seedItems[(existingIncidentCount + index) % seedItems.Count];
            var createdAt = DateTime.UtcNow.AddMinutes(-random.Next(0, 7 * 24 * 60 + 1));
            var selectedStatus = SeedStatuses[random.Next(SeedStatuses.Length)];
            var selectedSeverity = SeedSeverities[random.Next(SeedSeverities.Length)];

            var incident = new Incident(
                title: $"{template.Title} #{existingIncidentCount + index + 1}",
                description: template.Description,
                severity: selectedSeverity,
                createdBy: actingUserId);

            incident.CreatedAt = createdAt;
            incident.UpdatedAt = createdAt;
            incident.Status = selectedStatus;

            if (selectedStatus is IncidentStatus.Resolved or IncidentStatus.Closed)
            {
                var resolvedAt = createdAt.AddMinutes(random.Next(0, (int)Math.Max(1, (DateTime.UtcNow - createdAt).TotalMinutes) + 1));
                incident.ResolvedAt = resolvedAt;
                incident.UpdatedAt = resolvedAt;
            }

            if (random.Next(0, 2) == 1)
            {
                incident.AssignedTo = actingUserId;
            }

            incidents.Add(incident);
        }

        await dbContext.Incidents.AddRangeAsync(incidents, cancellationToken);

        var allIncidentIds = existingIncidentIds
            .Concat(incidents.Select(incident => incident.Id))
            .ToList();

        var existingLogCounts = await dbContext.IncidentLogs
            .AsNoTracking()
            .Where(log => allIncidentIds.Contains(log.IncidentId))
            .GroupBy(log => log.IncidentId)
            .Select(group => new { IncidentId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.IncidentId, item => item.Count, cancellationToken);

        var incidentLogs = new List<IncidentLog>(Math.Max(0, allIncidentIds.Count * logsPerIncident));
        var comments = new List<Comment>();

        foreach (var incidentId in allIncidentIds)
        {
            var existingLogsForIncident = existingLogCounts.GetValueOrDefault(incidentId, 0);
            var logsToCreate = Math.Max(0, logsPerIncident - existingLogsForIncident);

            for (var logIndex = 0; logIndex < logsToCreate; logIndex++)
            {
                var action = LogActionTemplates[random.Next(LogActionTemplates.Length)];
                incidentLogs.Add(new IncidentLog
                {
                    IncidentId = incidentId,
                    Action = action,
                    Details = $"{action} during seed generation ({existingLogsForIncident + logIndex + 1}/{logsPerIncident}).",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(5, 24 * 60)),
                    PerformedByUserId = actingUserId
                });
            }
        }

        foreach (var incident in incidents)
        {
            var commentCount = random.Next(0, 4);
            for (var commentIndex = 0; commentIndex < commentCount; commentIndex++)
            {
                comments.Add(new Comment
                {
                    IncidentId = incident.Id,
                    Content = CommentTemplates[random.Next(CommentTemplates.Length)],
                    CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(5, 24 * 60)),
                    CreatedByUserId = actingUserId
                });
            }
        }

        if (incidentLogs.Count > 0)
        {
            await dbContext.IncidentLogs.AddRangeAsync(incidentLogs, cancellationToken);
        }

        if (comments.Count > 0)
        {
            await dbContext.Comments.AddRangeAsync(comments, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded {IncidentCount} startup incidents and added {LogCount} incident logs with {CommentCount} comments.",
            incidents.Count,
            incidentLogs.Count,
            comments.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task<Guid> ResolveSeedUserIdAsync(
        IncidentFlowDbContext dbContext,
        Guid fallbackUserId,
        CancellationToken cancellationToken)
    {
        var configuredSeedUserExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == _seedOptions.SeedUserId, cancellationToken);

        if (configuredSeedUserExists)
        {
            return _seedOptions.SeedUserId;
        }

        _logger.LogWarning(
            "Configured IncidentSeed:SeedUserId {SeedUserId} was not found. Falling back to admin user for seed ownership.",
            _seedOptions.SeedUserId);

        return fallbackUserId;
    }
}