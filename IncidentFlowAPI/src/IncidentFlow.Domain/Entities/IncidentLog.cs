namespace IncidentFlow.Domain.Entities;

public class IncidentLog : BaseEntity
{
    public Guid IncidentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid PerformedByUserId { get; set; }


    // Navigation properties
    public Incident? Incident { get; set; }
    public User? PerformedByUser { get; set; }
}
