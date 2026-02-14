namespace IncidentFlow.API.Contracts.Users;

public sealed class UserResponseDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class UserCreateDto
{
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}

public sealed class UserUpdateDto
{
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? FullName { get; init; }
}