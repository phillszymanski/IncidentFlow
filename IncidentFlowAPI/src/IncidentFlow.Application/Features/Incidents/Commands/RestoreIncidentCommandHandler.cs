using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public class RestoreIncidentCommandHandler : IRequestHandler<RestoreIncidentCommand, bool>
{
    private readonly IIncidentRepository _repository;
    private readonly IIncidentLogRepository _incidentLogRepository;

    public RestoreIncidentCommandHandler(
        IIncidentRepository repository,
        IIncidentLogRepository incidentLogRepository)
    {
        _repository = repository;
        _incidentLogRepository = incidentLogRepository;
    }

    public async Task<bool> Handle(RestoreIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _repository.GetByIdIncludingDeletedAsync(request.Id, cancellationToken);
        if (incident is null || !incident.IsDeleted)
        {
            return false;
        }

        incident.IsDeleted = false;
        incident.DeletedAt = null;
        incident.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(incident, cancellationToken);

        await _incidentLogRepository.AddAsync(new IncidentLog
        {
            IncidentId = incident.Id,
            Action = "Restore",
            Details = "Incident restored from soft delete.",
            CreatedAt = DateTime.UtcNow,
            PerformedByUserId = request.PerformedByUserId
        }, cancellationToken);

        return true;
    }
}
