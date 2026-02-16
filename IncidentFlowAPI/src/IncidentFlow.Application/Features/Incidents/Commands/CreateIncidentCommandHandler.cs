using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public class CreateIncidentCommandHandler 
    : IRequestHandler<CreateIncidentCommand, Guid>
{
    private readonly IIncidentRepository _repository;
    private readonly IIncidentLogRepository _incidentLogRepository;

    public CreateIncidentCommandHandler(
        IIncidentRepository repository,
        IIncidentLogRepository incidentLogRepository)
    {
        _repository = repository;
        _incidentLogRepository = incidentLogRepository;
    }

    public async Task<Guid> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        var incident = new Incident(
            request.Title,
            request.Description,
            request.Severity,
            request.CreatedBy
        )
        {
            AssignedTo = request.AssignedTo
        };

        await _repository.AddAsync(incident, cancellationToken);

        var logDetails = $"Created incident with severity '{incident.Severity}'"
            + (incident.AssignedTo.HasValue ? $" and assignment to '{incident.AssignedTo}'" : ".");

        await _incidentLogRepository.AddAsync(new IncidentLog
        {
            IncidentId = incident.Id,
            Action = "Create",
            Details = logDetails,
            CreatedAt = DateTime.UtcNow,
            PerformedByUserId = request.CreatedBy
        }, cancellationToken);

        return incident.Id;
    }
}
