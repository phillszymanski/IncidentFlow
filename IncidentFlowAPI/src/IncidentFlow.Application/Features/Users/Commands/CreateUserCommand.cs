using MediatR;

namespace IncidentFlow.Application.Features.Users.Commands;

public record CreateUserCommand : IRequest<Guid>
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}