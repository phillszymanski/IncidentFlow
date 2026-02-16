using MediatR;

namespace IncidentFlow.Application.Features.Comments.Commands;

public record CreateCommentCommand : IRequest<Guid>
{
    public Guid IncidentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }
}
