using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public record DeleteIncidentCommand(Guid Id, Guid PerformedByUserId) : IRequest;