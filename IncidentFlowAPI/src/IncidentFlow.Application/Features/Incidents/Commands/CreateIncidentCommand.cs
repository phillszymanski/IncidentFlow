namespace IncidentFlow.Application.Features.Incidents.Commands;

using MediatR;

public record CreateIncidentCommand : IRequest<Guid>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public SeverityLevel Severity { get; init; }
    public Guid CreatedBy { get; init; }
    public Guid? AssignedTo { get; init; }
}