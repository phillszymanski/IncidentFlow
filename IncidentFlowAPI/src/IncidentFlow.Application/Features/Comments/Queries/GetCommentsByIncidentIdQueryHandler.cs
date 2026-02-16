using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Queries;

public class GetCommentsByIncidentIdQueryHandler : IRequestHandler<GetCommentsByIncidentIdQuery, List<Comment>>
{
    private readonly ICommentRepository _repository;

    public GetCommentsByIncidentIdQueryHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Comment>> Handle(GetCommentsByIncidentIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIncidentIdAsync(request.IncidentId, cancellationToken);
    }
}
