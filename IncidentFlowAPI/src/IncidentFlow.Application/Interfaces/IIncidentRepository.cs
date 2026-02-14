using IncidentFlow.Domain.Entities;

namespace IncidentFlow.Application.Interfaces
{
    public interface IIncidentRepository
    {
        // Add a new incident
        Task AddAsync(Incident incident, CancellationToken cancellationToken = default);

        // Update an existing incident
        Task UpdateAsync(Incident incident, CancellationToken cancellationToken = default);

        // Get incident by Id
        Task<Incident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        // List all incidents (optional: add paging/filters later)
        Task<List<Incident>> GetAllAsync(CancellationToken cancellationToken = default);

        // Example: get by status or severity
        Task<List<Incident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}