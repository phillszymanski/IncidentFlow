using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<User?>;