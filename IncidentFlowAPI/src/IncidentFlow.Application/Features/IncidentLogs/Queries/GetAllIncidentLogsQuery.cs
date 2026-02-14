using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Queries;

public record GetAllIncidentLogsQuery : IRequest<List<IncidentLog>>;