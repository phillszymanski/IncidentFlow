using IncidentFlow.Application.Interfaces;
using IncidentFlow.Application.Features.Incidents.Queries;
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

        public async Task<(List<Incident> Items, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            IncidentListFilterType filter,
            Guid? currentUserId,
            CancellationToken cancellationToken = default)
        {
            var normalizedPage = Math.Max(1, page);
            var normalizedPageSize = Math.Max(1, pageSize);
            var startOfWeek = GetStartOfWeekUtc(DateTime.UtcNow);

            var query = _dbContext.Incidents
                .AsNoTracking()
                .Where(i => !i.IsDeleted);

            query = filter switch
            {
                IncidentListFilterType.Open => query.Where(i => i.Status == IncidentStatus.Open),
                IncidentListFilterType.Critical => query.Where(i => i.Severity == SeverityLevel.Critical),
                IncidentListFilterType.ResolvedThisWeek => query.Where(i => i.ResolvedAt.HasValue && i.ResolvedAt.Value >= startOfWeek),
                IncidentListFilterType.Unassigned => query.Where(i => i.AssignedTo == null),
                IncidentListFilterType.AssignedToMe when currentUserId.HasValue => query.Where(i => i.AssignedTo == currentUserId),
                IncidentListFilterType.AssignedToMe => query.Where(i => false),
                _ => query
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Include(i => i.IncidentLogs)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        private static DateTime GetStartOfWeekUtc(DateTime currentUtc)
        {
            var day = (int)currentUtc.DayOfWeek;
            var diff = day == 0 ? -6 : 1 - day;
            return currentUtc.Date.AddDays(diff);
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
