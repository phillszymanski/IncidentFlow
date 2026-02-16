using IncidentFlow.Domain.Entities;

namespace IncidentFlow.Application.Interfaces;

public interface ICommentRepository
{
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Comment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Comment>> GetByIncidentIdAsync(Guid incidentId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
