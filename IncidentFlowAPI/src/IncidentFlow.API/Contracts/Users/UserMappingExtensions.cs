using IncidentFlow.Domain.Entities;

namespace IncidentFlow.API.Contracts.Users;

public static class UserMappingExtensions
{
    public static UserResponseDto ToResponseDto(this User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            CreatedAt = user.CreatedAt
        };
    }
}