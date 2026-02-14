using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Commands;

public class CreateIncidentCommandHandler 
    : IRequestHandler<CreateIncidentCommand, Guid>
{
    private readonly IIncidentRepository _repository;

    public CreateIncidentCommandHandler(IIncidentRepository repository)
    {
        _repository = repository;
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

        return incident.Id;
    }
}
