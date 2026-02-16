using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Infrastructure.Persistence.Repositories
{
    public class IncidentRepository : IIncidentRepository
    {
        private readonly IncidentFlowDbContext _dbContext;

        public IncidentRepository(IncidentFlowDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Incident incident, CancellationToken cancellationToken = default)
        {
            await _dbContext.Incidents.AddAsync(incident, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Incident incident, CancellationToken cancellationToken = default)
        {
            _dbContext.Incidents.Update(incident);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Incidents
                .Where(i => !i.IsDeleted)
                .Include(i => i.IncidentLogs)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<Incident?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Incidents
                .Include(i => i.IncidentLogs)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<List<Incident>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Incidents
                .Where(i => !i.IsDeleted)
                .Include(i => i.IncidentLogs)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Incident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Incidents
                .Where(i => i.Status == status && !i.IsDeleted)
                .Include(i => i.IncidentLogs)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var incident = await _dbContext.Incidents.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
            if (incident is null)
            {
                return;
            }

            if (!incident.IsDeleted)
            {
                incident.IsDeleted = true;
                incident.DeletedAt = DateTime.UtcNow;
                incident.UpdatedAt = DateTime.UtcNow;
                _dbContext.Incidents.Update(incident);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
