using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Queries;

public class GetIncidentLogByIdQueryHandler : IRequestHandler<GetIncidentLogByIdQuery, IncidentLog?>
{
    private readonly IIncidentLogRepository _repository;

    public GetIncidentLogByIdQueryHandler(IIncidentLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IncidentLog?> Handle(GetIncidentLogByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}