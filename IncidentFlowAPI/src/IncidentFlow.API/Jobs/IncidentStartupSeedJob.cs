using IncidentFlow.Domain.Entities;
using IncidentFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IncidentFlow.API.Jobs;

public sealed class IncidentStartupSeedJob : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IncidentStartupSeedJob> _logger;
    private readonly IncidentSeedOptions _seedOptions;

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

        await dbContext.Database.MigrateAsync(cancellationToken);

        var hasAnyIncidents = await dbContext.Incidents
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (hasAnyIncidents)
        {
            _logger.LogInformation("Skipping incident seed because records already exist.");
            return;
        }

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

        var incidents = seedItems
            .Select(item => new Incident(
                item.Title,
                item.Description,
                item.Severity,
                _seedOptions.SeedUserId))
            .ToList();

        await dbContext.Incidents.AddRangeAsync(incidents, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} startup incidents.", incidents.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}