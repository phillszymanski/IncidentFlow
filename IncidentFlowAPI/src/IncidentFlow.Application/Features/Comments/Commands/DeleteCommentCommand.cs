using MediatR;

namespace IncidentFlow.Application.Features.Comments.Commands;

public record DeleteCommentCommand(Guid Id) : IRequest;
