using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public class DeleteIncidentCommandHandler : IRequestHandler<DeleteIncidentCommand>
{
    private readonly IIncidentRepository _repository;
    private readonly IIncidentLogRepository _incidentLogRepository;

    public DeleteIncidentCommandHandler(
        IIncidentRepository repository,
        IIncidentLogRepository incidentLogRepository)
    {
        _repository = repository;
        _incidentLogRepository = incidentLogRepository;
    }

    public async Task Handle(DeleteIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (incident is null)
        {
            return;
        }

        incident.IsDeleted = true;
        incident.DeletedAt = DateTime.UtcNow;
        incident.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(incident, cancellationToken);

        await _incidentLogRepository.AddAsync(new IncidentLog
        {
            IncidentId = incident.Id,
            Action = "Delete (soft)",
            Details = "Incident soft deleted.",
            CreatedAt = DateTime.UtcNow,
            PerformedByUserId = request.PerformedByUserId
        }, cancellationToken);
    }
}