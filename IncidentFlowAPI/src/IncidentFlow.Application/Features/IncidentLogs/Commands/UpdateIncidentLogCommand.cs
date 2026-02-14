using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Commands;

public record UpdateIncidentLogCommand : IRequest<IncidentLog?>
{
    public Guid Id { get; init; }
    public Guid? IncidentId { get; init; }
    public string? Action { get; init; }
    public string? Details { get; init; }
    public Guid? PerformedByUserId { get; init; }
}