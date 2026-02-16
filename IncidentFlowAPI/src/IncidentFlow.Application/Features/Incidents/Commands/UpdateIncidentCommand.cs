using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public record UpdateIncidentCommand : IRequest<Incident?>
{
    public Guid Id { get; init; }
    public Guid PerformedByUserId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public IncidentStatus? Status { get; init; }
    public SeverityLevel? Severity { get; init; }
    public Guid? AssignedTo { get; init; }
    public DateTime? ResolvedAt { get; init; }
}