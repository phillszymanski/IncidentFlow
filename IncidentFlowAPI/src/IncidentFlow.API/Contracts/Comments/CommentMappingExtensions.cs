using IncidentFlow.Domain.Entities;

namespace IncidentFlow.API.Contracts.Comments;

public static class CommentMappingExtensions
{
    public static CommentResponseDto ToResponseDto(this Comment comment)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            IncidentId = comment.IncidentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            CreatedByUserId = comment.CreatedByUserId
        };
    }
}
