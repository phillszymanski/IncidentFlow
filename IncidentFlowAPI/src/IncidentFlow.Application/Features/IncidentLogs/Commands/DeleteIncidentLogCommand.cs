using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Commands;

public record DeleteIncidentLogCommand(Guid Id) : IRequest;