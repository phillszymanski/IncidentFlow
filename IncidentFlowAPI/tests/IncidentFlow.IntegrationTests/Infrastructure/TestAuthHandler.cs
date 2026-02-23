using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IncidentFlow.IntegrationTests.Infrastructure;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string UserIdHeader = "X-Test-UserId";
    public const string UsernameHeader = "X-Test-Username";
    public const string EmailHeader = "X-Test-Email";
    public const string RoleHeader = "X-Test-Role";
    public const string PermissionsHeader = "X-Test-Permissions";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdHeader) || string.IsNullOrWhiteSpace(userIdHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Guid.TryParse(userIdHeader.ToString(), out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid test auth user id."));
        }

        var username = Request.Headers.TryGetValue(UsernameHeader, out var usernameHeader)
            ? usernameHeader.ToString()
            : "test-user";
        var email = Request.Headers.TryGetValue(EmailHeader, out var emailHeader)
            ? emailHeader.ToString()
            : "test-user@test.local";
        var role = Request.Headers.TryGetValue(RoleHeader, out var roleHeader)
            ? roleHeader.ToString()
            : "User";

        var claims = new List<Claim>
        {
            new("userId", userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName, username),
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, email)
        };

        if (Request.Headers.TryGetValue(PermissionsHeader, out var permissionsHeader) && !string.IsNullOrWhiteSpace(permissionsHeader))
        {
            var permissions = permissionsHeader.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
