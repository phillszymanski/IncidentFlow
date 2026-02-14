using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public class UpdateIncidentCommandHandler : IRequestHandler<UpdateIncidentCommand, Incident?>
{
    private readonly IIncidentRepository _repository;

    public UpdateIncidentCommandHandler(IIncidentRepository repository)
    {
        _repository = repository;
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
            incident.Status = request.Status.Value;
        }

        if (request.Severity.HasValue)
        {
            incident.Severity = request.Severity.Value;
        }

        if (request.AssignedTo.HasValue)
        {
            incident.AssignedTo = request.AssignedTo;
        }

        if (request.ResolvedAt.HasValue)
        {
            incident.ResolvedAt = request.ResolvedAt;
        }

        await _repository.UpdateAsync(incident, cancellationToken);
        return incident;
    }
}