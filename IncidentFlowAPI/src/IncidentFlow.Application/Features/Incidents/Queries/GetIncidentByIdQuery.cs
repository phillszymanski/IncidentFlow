using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Queries;

public record GetIncidentByIdQuery(Guid Id) : IRequest<Incident?>;