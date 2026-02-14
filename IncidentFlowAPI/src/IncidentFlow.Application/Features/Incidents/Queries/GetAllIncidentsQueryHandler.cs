using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Queries;

public class GetAllIncidentsQueryHandler : IRequestHandler<GetAllIncidentsQuery, List<Incident>>
{
    private readonly IIncidentRepository _repository;

    public GetAllIncidentsQueryHandler(IIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Incident>> Handle(GetAllIncidentsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}