using IncidentFlow.Domain.Entities;

namespace IncidentFlow.API.Jobs;

public sealed class IncidentSeedOptions
{
    public bool Enabled { get; set; } = true;
    public Guid SeedUserId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public List<IncidentSeedItem> Items { get; set; } = [];
}

public sealed class IncidentSeedItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SeverityLevel Severity { get; set; } = SeverityLevel.Low;
}