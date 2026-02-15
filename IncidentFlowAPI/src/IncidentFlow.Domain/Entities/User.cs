namespace IncidentFlow.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}