using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Commands;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Guid>
{
    private readonly ICommentRepository _repository;

    public CreateCommentCommandHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            IncidentId = request.IncidentId,
            Content = request.Content,
            CreatedByUserId = request.CreatedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(comment, cancellationToken);
        return comment.Id;
    }
}
