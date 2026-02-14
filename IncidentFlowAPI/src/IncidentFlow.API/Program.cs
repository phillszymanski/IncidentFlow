using IncidentFlow.Application.Features.Incidents.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Infrastructure.Persistence;
using IncidentFlow.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateIncidentCommand).Assembly));

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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
