using MediatR;

namespace IncidentFlow.Application.Features.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest;