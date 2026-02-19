using IncidentFlow.Application.Interfaces;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Queries;

public class GetAllIncidentsQueryHandler : IRequestHandler<GetAllIncidentsQuery, PaginatedIncidentsResult>
{
    private readonly IIncidentRepository _repository;

    public GetAllIncidentsQueryHandler(IIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedIncidentsResult> Handle(GetAllIncidentsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, request.PageSize);
        var (items, totalCount) = await _repository.GetPagedAsync(
            page,
            pageSize,
            request.Filter,
            request.CurrentUserId,
            cancellationToken);
        return new PaginatedIncidentsResult(items, totalCount, page, pageSize);
    }
}