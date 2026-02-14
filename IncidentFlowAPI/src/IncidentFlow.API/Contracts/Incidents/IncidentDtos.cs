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
    public DateTime? ResolvedAt { get; init; }
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