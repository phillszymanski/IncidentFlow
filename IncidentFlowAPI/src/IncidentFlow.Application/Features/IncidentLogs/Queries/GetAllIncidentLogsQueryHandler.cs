using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.IncidentLogs.Queries;

public class GetAllIncidentLogsQueryHandler : IRequestHandler<GetAllIncidentLogsQuery, List<IncidentLog>>
{
    private readonly IIncidentLogRepository _repository;

    public GetAllIncidentLogsQueryHandler(IIncidentLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<IncidentLog>> Handle(GetAllIncidentLogsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}