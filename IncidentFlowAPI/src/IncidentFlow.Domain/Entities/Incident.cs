namespace IncidentFlow.Domain.Entities;

public class Incident : BaseEntity
{
    public Incident(string title, string description, SeverityLevel severity, Guid createdBy)
    {
        Title = title;
        Description = description;
        Severity = severity;
        Status = IncidentStatus.Open;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        CreatedBy = createdBy;

        AddDomainEvent(new IncidentCreatedEvent(Guid.NewGuid()));
    }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public IncidentStatus Status { get; set; }
    public SeverityLevel Severity { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? AssignedTo { get; set; }
    public List<IncidentLog> IncidentLogs { get; set; } = new List<IncidentLog>();
}