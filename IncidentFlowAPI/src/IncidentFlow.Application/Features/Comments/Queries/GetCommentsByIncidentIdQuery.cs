using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Queries;

public record GetCommentsByIncidentIdQuery(Guid IncidentId) : IRequest<List<Comment>>;
