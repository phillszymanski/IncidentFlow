using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public record RestoreIncidentCommand(Guid Id, Guid PerformedByUserId) : IRequest<bool>;
