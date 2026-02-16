using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Infrastructure.Persistence.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IncidentFlowDbContext _dbContext;

    public CommentRepository(IncidentFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Comments.AddAsync(comment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _dbContext.Comments.Update(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .Include(x => x.Incident)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Comment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .Include(x => x.Incident)
            .Include(x => x.CreatedByUser)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Comment>> GetByIncidentIdAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .Where(x => x.IncidentId == incidentId)
            .Include(x => x.CreatedByUser)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await _dbContext.Comments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (comment is null)
        {
            return;
        }

        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
