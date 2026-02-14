using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Commands;

public class CreateIncidentLogCommandHandler : IRequestHandler<CreateIncidentLogCommand, Guid>
{
    private readonly IIncidentLogRepository _repository;

    public CreateIncidentLogCommandHandler(IIncidentLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateIncidentLogCommand request, CancellationToken cancellationToken)
    {
        var incidentLog = new IncidentLog
        {
            IncidentId = request.IncidentId,
            Action = request.Action,
            Details = request.Details,
            PerformedByUserId = request.PerformedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(incidentLog, cancellationToken);
        return incidentLog.Id;
    }
}