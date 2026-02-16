using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IncidentFlow.API.Authorization;
using IncidentFlow.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IncidentFlow.API.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string CreateToken(User user)
    {
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("userId", user.Id.ToString())
        };

        claims.AddRange(GetPermissionsForRole(user.Role).Select(permission => new Claim("permission", permission)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public IReadOnlyCollection<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            "Admin" =>
            [
                PermissionConstants.IncidentsRead,
                PermissionConstants.IncidentsCreate,
                PermissionConstants.IncidentsEditAny,
                PermissionConstants.IncidentsStatusAny,
                PermissionConstants.IncidentsSeverityAny,
                PermissionConstants.IncidentsAssign,
                PermissionConstants.IncidentsDelete,
                PermissionConstants.IncidentsRestore,
                PermissionConstants.IncidentsAuditRead,
                PermissionConstants.UsersManage,
                PermissionConstants.RolesManage,
                PermissionConstants.DashboardBasic,
                PermissionConstants.DashboardFull
            ],
            "Manager" or "Responder" =>
            [
                PermissionConstants.IncidentsRead,
                PermissionConstants.IncidentsCreate,
                PermissionConstants.IncidentsEditAny,
                PermissionConstants.IncidentsStatusAny,
                PermissionConstants.IncidentsSeverityAny,
                PermissionConstants.IncidentsAssign,
                PermissionConstants.DashboardBasic,
                PermissionConstants.DashboardFull
            ],
            "User" =>
            [
                PermissionConstants.IncidentsRead,
                PermissionConstants.IncidentsCreate,
                PermissionConstants.IncidentsEditOwn,
                PermissionConstants.IncidentsStatusLimited,
                PermissionConstants.DashboardBasic
            ],
            _ =>
            [
                PermissionConstants.IncidentsRead,
                PermissionConstants.DashboardBasic
            ]
        };
    }
}
