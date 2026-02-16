using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Queries;

public class GetIncidentLogsByIncidentIdQueryHandler : IRequestHandler<GetIncidentLogsByIncidentIdQuery, List<IncidentLog>>
{
    private readonly IIncidentLogRepository _repository;

    public GetIncidentLogsByIncidentIdQueryHandler(IIncidentLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IncidentLog>> Handle(GetIncidentLogsByIncidentIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIncidentIdAsync(request.IncidentId, cancellationToken);
    }
}
