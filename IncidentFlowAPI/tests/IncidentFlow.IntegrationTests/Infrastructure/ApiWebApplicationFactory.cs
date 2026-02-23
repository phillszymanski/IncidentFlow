using IncidentFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IncidentFlow.IntegrationTests.Infrastructure;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"IncidentFlow.IntegrationTests.{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-test-secret-with-at-least-32-characters",
                ["Jwt:Issuer"] = "IncidentFlowAPI",
                ["Jwt:Audience"] = "IncidentFlowClient",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Jwt:CookieName"] = "incidentflow_access",
                ["Jwt:CsrfCookieName"] = "incidentflow_csrf",
                ["Jwt:CsrfHeaderName"] = "X-CSRF-TOKEN",
                ["IncidentSeed:Enabled"] = "false",
                ["ConnectionStrings:DefaultConnection"] = "Host=unused;Database=unused;Username=unused;Password=unused"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.RemoveAll<DbContextOptions<IncidentFlowDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<IncidentFlowDbContext>>();
            services.RemoveAll<IncidentFlowDbContext>();

            services.AddDbContext<IncidentFlowDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
