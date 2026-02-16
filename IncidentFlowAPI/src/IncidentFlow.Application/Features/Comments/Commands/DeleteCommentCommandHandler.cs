using IncidentFlow.Application.Interfaces;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Commands;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly ICommentRepository _repository;

    public DeleteCommentCommandHandler(ICommentRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id, cancellationToken);
    }
}
