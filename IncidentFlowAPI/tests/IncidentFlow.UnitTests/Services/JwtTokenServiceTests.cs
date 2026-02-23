using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IncidentFlow.API.Authorization;
using IncidentFlow.API.Services;
using IncidentFlow.Domain.Entities;
using Microsoft.Extensions.Options;

namespace IncidentFlow.UnitTests.Services;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(int expiryMinutes = 60)
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "super-secure-test-secret-with-at-least-32-characters",
            Issuer = "IncidentFlowAPI",
            Audience = "IncidentFlowClient",
            ExpiryMinutes = expiryMinutes
        });

        return new JwtTokenService(options);
    }

    [Fact]
    public void CreateToken_ContainsExpectedIdentityClaims_AndPermissions()
    {
        var service = CreateService();
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
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == "userId" && claim.Value == user.Id.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.UniqueName && claim.Value == "admin-user");
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == "admin@test.com");
        Assert.Contains(jwt.Claims, claim => claim.Type == "permission" && claim.Value == "users:manage");
    }

    [Fact]
    public void GetPermissionsForRole_ReturnsBaselinePermissions_ForUnknownRole()
    {
        var service = CreateService();

        var permissions = service.GetPermissionsForRole("UnknownRole");

        Assert.Contains("incidents:read", permissions);
        Assert.Contains("dashboard:basic", permissions);
        Assert.DoesNotContain("users:manage", permissions);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Responder")]
    public void GetPermissionsForRole_ReturnsExpectedPermissionSet_ForManagerAndResponder(string role)
    {
        var service = CreateService();

        var permissions = service.GetPermissionsForRole(role).ToHashSet();

        Assert.Equal(8, permissions.Count);
        Assert.Contains(PermissionConstants.IncidentsRead, permissions);
        Assert.Contains(PermissionConstants.IncidentsCreate, permissions);
        Assert.Contains(PermissionConstants.IncidentsEditAny, permissions);
        Assert.Contains(PermissionConstants.IncidentsStatusAny, permissions);
        Assert.Contains(PermissionConstants.IncidentsSeverityAny, permissions);
        Assert.Contains(PermissionConstants.IncidentsAssign, permissions);
        Assert.Contains(PermissionConstants.DashboardBasic, permissions);
        Assert.Contains(PermissionConstants.DashboardFull, permissions);
        Assert.DoesNotContain(PermissionConstants.UsersManage, permissions);
    }

    [Fact]
    public void GetPermissionsForRole_ReturnsExpectedPermissionSet_ForUserRole()
    {
        var service = CreateService();

        var permissions = service.GetPermissionsForRole("User").ToHashSet();

        Assert.Equal(5, permissions.Count);
        Assert.Contains(PermissionConstants.IncidentsRead, permissions);
        Assert.Contains(PermissionConstants.IncidentsCreate, permissions);
        Assert.Contains(PermissionConstants.IncidentsEditOwn, permissions);
        Assert.Contains(PermissionConstants.IncidentsStatusLimited, permissions);
        Assert.Contains(PermissionConstants.DashboardBasic, permissions);
        Assert.DoesNotContain(PermissionConstants.IncidentsEditAny, permissions);
        Assert.DoesNotContain(PermissionConstants.UsersManage, permissions);
    }

    [Fact]
    public void CreateToken_UsesConfiguredExpiryWindow()
    {
        var service = CreateService(expiryMinutes: 15);
        var user = new User
        {
            Username = "timed-user",
            Email = "timed@test.com",
            FullName = "Timed User",
            Role = "User"
        };

        var before = DateTime.UtcNow;
        var token = service.CreateToken(user);
        var after = DateTime.UtcNow;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.InRange(jwt.ValidFrom, before.AddSeconds(-2), after.AddSeconds(2));
        Assert.InRange(jwt.ValidTo, before.AddMinutes(15).AddSeconds(-2), after.AddMinutes(15).AddSeconds(2));
    }
}
