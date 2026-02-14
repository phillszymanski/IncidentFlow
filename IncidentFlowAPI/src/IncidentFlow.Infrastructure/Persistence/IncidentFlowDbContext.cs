using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Infrastructure.Persistence
{
    public class IncidentFlowDbContext : DbContext, IIncidentFlowDbContext
    {
        public IncidentFlowDbContext(DbContextOptions<IncidentFlowDbContext> options)
            : base(options)
        {
        }

        // Entities
        public DbSet<Incident> Incidents => Set<Incident>();
        public DbSet<User> Users => Set<User>();
        public DbSet<IncidentLog> IncidentLogs => Set<IncidentLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations (if any)
            // modelBuilder.ApplyConfigurationsFromAssembly(typeof(IncidentFlowDbContext).Assembly);
        }
    }
}
