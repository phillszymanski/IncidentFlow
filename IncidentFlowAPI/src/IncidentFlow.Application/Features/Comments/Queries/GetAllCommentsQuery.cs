using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Comments.Queries;

public record GetAllCommentsQuery : IRequest<List<Comment>>;
