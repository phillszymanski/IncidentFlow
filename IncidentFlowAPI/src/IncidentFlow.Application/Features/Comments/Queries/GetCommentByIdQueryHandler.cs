using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Queries;

public class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, Comment?>
{
    private readonly ICommentRepository _repository;

    public GetCommentByIdQueryHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Comment?> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
