public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}