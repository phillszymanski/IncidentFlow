using IncidentFlow.Domain.Entities;

namespace IncidentFlow.API.Services;

public interface IJwtTokenService
{
    string CreateToken(User user);
    IReadOnlyCollection<string> GetPermissionsForRole(string role);
}
