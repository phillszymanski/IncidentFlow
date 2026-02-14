using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Commands;

public class UpdateIncidentLogCommandHandler : IRequestHandler<UpdateIncidentLogCommand, IncidentLog?>
{
    private readonly IIncidentLogRepository _repository;

    public UpdateIncidentLogCommandHandler(IIncidentLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IncidentLog?> Handle(UpdateIncidentLogCommand request, CancellationToken cancellationToken)
    {
        var incidentLog = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (incidentLog is null)
        {
            return null;
        }

        if (request.IncidentId.HasValue)
        {
            incidentLog.IncidentId = request.IncidentId.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            incidentLog.Action = request.Action;
        }

        if (!string.IsNullOrWhiteSpace(request.Details))
        {
            incidentLog.Details = request.Details;
        }

        if (request.PerformedByUserId.HasValue)
        {
            incidentLog.PerformedByUserId = request.PerformedByUserId.Value;
        }

        await _repository.UpdateAsync(incidentLog, cancellationToken);
        return incidentLog;
    }
}