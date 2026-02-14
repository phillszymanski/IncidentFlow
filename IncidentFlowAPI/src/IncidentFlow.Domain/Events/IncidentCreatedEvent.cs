public record IncidentCreatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredAt { get; }
    public string EventType => nameof(IncidentCreatedEvent);

    public IncidentCreatedEvent(Guid incidentId, DateTime occurredAt = default)
    {
        Id = incidentId;
        OccurredAt = occurredAt == default ? DateTime.UtcNow : occurredAt;
    }
}