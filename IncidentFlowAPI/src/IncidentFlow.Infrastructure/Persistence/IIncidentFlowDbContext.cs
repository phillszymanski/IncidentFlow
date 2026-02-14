using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Application.Interfaces
{
    public interface IIncidentFlowDbContext
    {
        DbSet<Incident> Incidents { get; }
        DbSet<User> Users { get; }
        DbSet<IncidentLog> IncidentLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}