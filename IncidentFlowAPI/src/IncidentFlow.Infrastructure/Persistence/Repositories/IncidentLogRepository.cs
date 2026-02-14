using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Infrastructure.Persistence.Repositories;

public class IncidentLogRepository : IIncidentLogRepository
{
    private readonly IncidentFlowDbContext _dbContext;

    public IncidentLogRepository(IncidentFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(IncidentLog incidentLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.IncidentLogs.AddAsync(incidentLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(IncidentLog incidentLog, CancellationToken cancellationToken = default)
    {
        _dbContext.IncidentLogs.Update(incidentLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IncidentLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.IncidentLogs
            .Include(x => x.Incident)
            .Include(x => x.PerformedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<IncidentLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.IncidentLogs
            .Include(x => x.Incident)
            .Include(x => x.PerformedByUser)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var incidentLog = await _dbContext.IncidentLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (incidentLog is null)
        {
            return;
        }

        _dbContext.IncidentLogs.Remove(incidentLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}