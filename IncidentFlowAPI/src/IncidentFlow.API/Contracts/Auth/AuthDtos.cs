namespace IncidentFlow.API.Contracts.Auth;

public sealed class LoginRequestDto
{
    public string UsernameOrEmail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class AuthUserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

public sealed class LoginResponseDto
{
    public AuthUserDto User { get; init; } = new();
}
