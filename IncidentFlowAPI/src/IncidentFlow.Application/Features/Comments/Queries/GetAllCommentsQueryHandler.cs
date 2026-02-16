using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Queries;

public class GetAllCommentsQueryHandler : IRequestHandler<GetAllCommentsQuery, List<Comment>>
{
    private readonly ICommentRepository _repository;

    public GetAllCommentsQueryHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Comment>> Handle(GetAllCommentsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
