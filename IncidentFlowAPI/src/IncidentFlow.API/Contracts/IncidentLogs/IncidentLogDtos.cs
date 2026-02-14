namespace IncidentFlow.API.Contracts.IncidentLogs;

public sealed class IncidentLogResponseDto
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid PerformedByUserId { get; init; }
}

public sealed class IncidentLogCreateDto
{
    public Guid IncidentId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public Guid PerformedByUserId { get; init; }
}

public sealed class IncidentLogUpdateDto
{
    public Guid? IncidentId { get; init; }
    public string? Action { get; init; }
    public string? Details { get; init; }
    public Guid? PerformedByUserId { get; init; }
}