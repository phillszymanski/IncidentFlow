using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Commands;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, Comment?>
{
    private readonly ICommentRepository _repository;

    public UpdateCommentCommandHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Comment?> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (comment is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            comment.Content = request.Content;
        }

        await _repository.UpdateAsync(comment, cancellationToken);
        return comment;
    }
}
