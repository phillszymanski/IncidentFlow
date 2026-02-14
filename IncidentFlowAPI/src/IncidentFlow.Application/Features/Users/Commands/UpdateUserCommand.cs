using IncidentFlow.Domain.Entities;
using MediatR;

namespace IncidentFlow.Application.Features.Users.Commands;

public record UpdateUserCommand : IRequest<User?>
{
    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? FullName { get; init; }
}