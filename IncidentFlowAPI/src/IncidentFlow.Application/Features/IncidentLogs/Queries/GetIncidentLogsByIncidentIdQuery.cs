using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Queries;

public record GetIncidentLogsByIncidentIdQuery(Guid IncidentId) : IRequest<List<IncidentLog>>;
