namespace IncidentFlow.API.Contracts.Incidents;

public sealed class IncidentResponseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IncidentStatus Status { get; init; }
    public SeverityLevel Severity { get; init; }
    public Guid CreatedBy { get; init; }
    public Guid? AssignedTo { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

public sealed class PagedIncidentResponseDto
{
    public IReadOnlyList<IncidentResponseDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public sealed class DashboardCountItemDto
{
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed class IncidentDashboardSummaryDto
{
    public int TotalIncidents { get; init; }
    public int OpenIncidents { get; init; }
    public int CriticalIncidents { get; init; }
    public int ResolvedThisWeek { get; init; }
    public int UnassignedIncidents { get; init; }
    public int AssignedToMeIncidents { get; init; }
    public IReadOnlyList<DashboardCountItemDto> Severity { get; init; } = [];
    public IReadOnlyList<DashboardCountItemDto> Status { get; init; } = [];
    public IReadOnlyList<DashboardCountItemDto> Trend { get; init; } = [];
}

public sealed class IncidentCreateDto
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SeverityLevel Severity { get; init; }
    public Guid CreatedBy { get; init; }
    public Guid? AssignedTo { get; init; }
}

public sealed class IncidentUpdateDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public IncidentStatus? Status { get; init; }
    public SeverityLevel? Severity { get; init; }
    public Guid? AssignedTo { get; init; }
    public DateTime? ResolvedAt { get; init; }
}