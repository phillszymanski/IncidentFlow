namespace IncidentFlow.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid IncidentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }

    public Incident? Incident { get; set; }
    public User? CreatedByUser { get; set; }
}
