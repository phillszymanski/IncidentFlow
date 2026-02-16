using IncidentFlow.Application.Features.Incidents.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.API.Authorization;
using IncidentFlow.API.Jobs;
using IncidentFlow.API.Services;
using IncidentFlow.Infrastructure.Persistence;
using IncidentFlow.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateIncidentCommand).Assembly));
builder.Services.Configure<IncidentSeedOptions>(builder.Configuration.GetSection("IncidentSeed"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("Jwt configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
{
    throw new InvalidOperationException("Jwt:Secret must be configured.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(jwtOptions.CookieName, out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyConstants.CanReadIncidents, policy => policy.RequireClaim("permission", PermissionConstants.IncidentsRead));
    options.AddPolicy(PolicyConstants.CanCreateIncidents, policy => policy.RequireClaim("permission", PermissionConstants.IncidentsCreate));
    options.AddPolicy(PolicyConstants.CanAssignIncidents, policy => policy.RequireClaim("permission", PermissionConstants.IncidentsAssign));
    options.AddPolicy("CanWriteIncidents", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("permission", PermissionConstants.IncidentsEditAny) ||
        context.User.HasClaim("permission", PermissionConstants.IncidentsEditOwn) ||
        context.User.HasClaim("permission", PermissionConstants.IncidentsCreate)));
    options.AddPolicy(PolicyConstants.CanManageUsers, policy => policy.RequireClaim("permission", PermissionConstants.UsersManage));
    options.AddPolicy(PolicyConstants.CanReadAuditLogs, policy => policy.RequireClaim("permission", PermissionConstants.IncidentsAuditRead));
    options.AddPolicy(PolicyConstants.CanDeleteIncidents, policy => policy.RequireClaim("permission", PermissionConstants.IncidentsDelete));
    options.AddPolicy(PolicyConstants.CanRestoreIncidents, policy => policy.RequireClaim("permission", PermissionConstants.IncidentsRestore));
});

builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<IncidentFlowDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IIncidentFlowDbContext>(provider =>
    provider.GetRequiredService<IncidentFlowDbContext>());

builder.Services.AddScoped<IIncidentRepository>(provider =>
    new IncidentRepository(provider.GetRequiredService<IncidentFlowDbContext>()));

builder.Services.AddScoped<IUserRepository>(provider =>
    new UserRepository(provider.GetRequiredService<IncidentFlowDbContext>()));

builder.Services.AddScoped<IIncidentLogRepository>(provider =>
    new IncidentLogRepository(provider.GetRequiredService<IncidentFlowDbContext>()));

builder.Services.AddScoped<ICommentRepository>(provider =>
    new CommentRepository(provider.GetRequiredService<IncidentFlowDbContext>()));

builder.Services.AddHostedService<IncidentStartupSeedJob>();

    builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
