using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Commands;

public record UpdateCommentCommand : IRequest<Comment?>
{
    public Guid Id { get; init; }
    public string? Content { get; init; }
}
