using IncidentFlow.Domain.Entities;

namespace IncidentFlow.API.Jobs;

public sealed class IncidentSeedOptions
{
    public bool Enabled { get; set; } = true;
    public bool SeedAdminUser { get; set; } = false;
    public string? AdminUsername { get; set; }
    public string? AdminEmail { get; set; }
    public string? AdminPassword { get; set; }
    public List<IncidentSeedUser> Users { get; set; } = [];
    public Guid SeedUserId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public int NumberOfIncidents { get; set; } = 25;
    public int IncidentLogsPerIncident { get; set; } = 5;
    public List<IncidentSeedItem> Items { get; set; } = [];
}

public sealed class IncidentSeedUser
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? FullName { get; set; }
}

public sealed class IncidentSeedItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SeverityLevel Severity { get; set; } = SeverityLevel.Low;
}