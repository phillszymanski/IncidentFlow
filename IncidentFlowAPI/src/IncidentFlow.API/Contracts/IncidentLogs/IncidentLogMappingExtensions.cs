using IncidentFlow.Domain.Entities;

namespace IncidentFlow.API.Contracts.IncidentLogs;

public static class IncidentLogMappingExtensions
{
    public static IncidentLogResponseDto ToResponseDto(this IncidentLog incidentLog)
    {
        return new IncidentLogResponseDto
        {
            Id = incidentLog.Id,
            IncidentId = incidentLog.IncidentId,
            Action = incidentLog.Action,
            Details = incidentLog.Details,
            CreatedAt = incidentLog.CreatedAt,
            PerformedByUserId = incidentLog.PerformedByUserId
        };
    }
}