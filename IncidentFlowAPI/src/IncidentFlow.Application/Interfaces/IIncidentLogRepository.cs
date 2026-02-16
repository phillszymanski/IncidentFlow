using IncidentFlow.Domain.Entities;

namespace IncidentFlow.Application.Interfaces;

public interface IIncidentLogRepository
{
    Task AddAsync(IncidentLog incidentLog, CancellationToken cancellationToken = default);
    Task UpdateAsync(IncidentLog incidentLog, CancellationToken cancellationToken = default);
    Task<IncidentLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<IncidentLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<IncidentLog>> GetByIncidentIdAsync(Guid incidentId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}