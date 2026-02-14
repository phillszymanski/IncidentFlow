using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Commands;

public record CreateIncidentLogCommand : IRequest<Guid>
{
    public Guid IncidentId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
    public Guid PerformedByUserId { get; init; }
}