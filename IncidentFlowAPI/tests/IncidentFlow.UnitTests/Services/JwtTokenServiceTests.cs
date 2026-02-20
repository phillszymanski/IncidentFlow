using System.IdentityModel.Tokens.Jwt;
using IncidentFlow.API.Services;
using IncidentFlow.Domain.Entities;
using Microsoft.Extensions.Options;

namespace IncidentFlow.UnitTests.Services;

public class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_ContainsExpectedIdentityClaims_AndPermissions()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "super-secure-test-secret-with-at-least-32-characters",
            Issuer = "IncidentFlowAPI",
            Audience = "IncidentFlowClient",
            ExpiryMinutes = 60
        });

        var service = new JwtTokenService(options);
        var user = new User
        {
            Username = "admin-user",
            Email = "admin@test.com",
            FullName = "Admin User",
            Role = "Admin"
        };

        var token = service.CreateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("IncidentFlowAPI", jwt.Issuer);
        Assert.Contains("IncidentFlowClient", jwt.Audiences);
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.UniqueName && claim.Value == "admin-user");
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == "admin@test.com");
        Assert.Contains(jwt.Claims, claim => claim.Type == "permission" && claim.Value == "users:manage");
    }

    [Fact]
    public void GetPermissionsForRole_ReturnsBaselinePermissions_ForUnknownRole()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "super-secure-test-secret-with-at-least-32-characters",
            Issuer = "IncidentFlowAPI",
            Audience = "IncidentFlowClient"
        });

        var service = new JwtTokenService(options);

        var permissions = service.GetPermissionsForRole("UnknownRole");

        Assert.Contains("incidents:read", permissions);
        Assert.Contains("dashboard:basic", permissions);
        Assert.DoesNotContain("users:manage", permissions);
    }
}
