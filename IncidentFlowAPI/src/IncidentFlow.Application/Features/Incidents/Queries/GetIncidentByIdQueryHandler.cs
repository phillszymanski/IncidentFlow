using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Queries;

public class GetIncidentByIdQueryHandler : IRequestHandler<GetIncidentByIdQuery, Incident?>
{
    private readonly IIncidentRepository _repository;

    public GetIncidentByIdQueryHandler(IIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Incident?> Handle(GetIncidentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}