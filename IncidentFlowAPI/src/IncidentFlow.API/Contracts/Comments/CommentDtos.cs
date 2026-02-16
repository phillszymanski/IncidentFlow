namespace IncidentFlow.API.Contracts.Comments;

public sealed class CommentResponseDto
{
    public Guid Id { get; init; }
    public Guid IncidentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid CreatedByUserId { get; init; }
}

public sealed class CommentCreateDto
{
    public Guid IncidentId { get; init; }
    public string Content { get; init; } = string.Empty;
}

public sealed class CommentUpdateDto
{
    public string? Content { get; init; }
}
