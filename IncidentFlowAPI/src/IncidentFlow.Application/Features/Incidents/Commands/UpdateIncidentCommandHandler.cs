using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public class UpdateIncidentCommandHandler : IRequestHandler<UpdateIncidentCommand, Incident?>
{
    private readonly IIncidentRepository _repository;
    private readonly IIncidentLogRepository _incidentLogRepository;

    public UpdateIncidentCommandHandler(
        IIncidentRepository repository,
        IIncidentLogRepository incidentLogRepository)
    {
        _repository = repository;
        _incidentLogRepository = incidentLogRepository;
    }

    public async Task<Incident?> Handle(UpdateIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (incident is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            incident.Title = request.Title;
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            incident.Description = request.Description;
        }

        if (request.Status.HasValue)
        {
            var previousStatus = incident.Status;
            incident.Status = request.Status.Value;

            if (previousStatus != incident.Status)
            {
                await _incidentLogRepository.AddAsync(new IncidentLog
                {
                    IncidentId = incident.Id,
                    Action = "Status change",
                    Details = $"Status changed from '{previousStatus}' to '{incident.Status}'.",
                    CreatedAt = DateTime.UtcNow,
                    PerformedByUserId = request.PerformedByUserId
                }, cancellationToken);
            }
        }

        if (request.Severity.HasValue)
        {
            var previousSeverity = incident.Severity;
            incident.Severity = request.Severity.Value;

            if (previousSeverity != incident.Severity)
            {
                await _incidentLogRepository.AddAsync(new IncidentLog
                {
                    IncidentId = incident.Id,
                    Action = "Severity change",
                    Details = $"Severity changed from '{previousSeverity}' to '{incident.Severity}'.",
                    CreatedAt = DateTime.UtcNow,
                    PerformedByUserId = request.PerformedByUserId
                }, cancellationToken);
            }
        }

        if (request.AssignedTo.HasValue)
        {
            var previousAssignee = incident.AssignedTo;
            incident.AssignedTo = request.AssignedTo;

            if (previousAssignee != incident.AssignedTo)
            {
                await _incidentLogRepository.AddAsync(new IncidentLog
                {
                    IncidentId = incident.Id,
                    Action = "Assignment change",
                    Details = $"Assignment changed from '{previousAssignee}' to '{incident.AssignedTo}'.",
                    CreatedAt = DateTime.UtcNow,
                    PerformedByUserId = request.PerformedByUserId
                }, cancellationToken);
            }
        }

        if (request.ResolvedAt.HasValue)
        {
            incident.ResolvedAt = request.ResolvedAt;
        }

        incident.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(incident, cancellationToken);
        return incident;
    }
}