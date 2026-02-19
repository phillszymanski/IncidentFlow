using IncidentFlow.Domain.Entities;
using IncidentFlow.Application.Features.Incidents.Queries;

namespace IncidentFlow.API.Contracts.Incidents;

public static class IncidentMappingExtensions
{
    public static IncidentResponseDto ToResponseDto(this Incident incident)
    {
        return new IncidentResponseDto
        {
            Id = incident.Id,
            Title = incident.Title,
            Description = incident.Description,
            Status = incident.Status,
            Severity = incident.Severity,
            CreatedBy = incident.CreatedBy,
            AssignedTo = incident.AssignedTo,
            CreatedAt = incident.CreatedAt,
            UpdatedAt = incident.UpdatedAt,
            ResolvedAt = incident.ResolvedAt
        };
    }

    public static void ApplyTo(this IncidentUpdateDto dto, Incident incident)
    {
        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            incident.Title = dto.Title;
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
        {
            incident.Description = dto.Description;
        }

        if (dto.Status.HasValue)
        {
            incident.Status = dto.Status.Value;
        }

        if (dto.Severity.HasValue)
        {
            incident.Severity = dto.Severity.Value;
        }

        if (dto.AssignedTo.HasValue)
        {
            incident.AssignedTo = dto.AssignedTo;
        }

        if (dto.ResolvedAt.HasValue)
        {
            incident.ResolvedAt = dto.ResolvedAt;
        }
    }

    public static IncidentDashboardSummaryDto ToResponseDto(this IncidentDashboardSummaryResult summary)
    {
        return new IncidentDashboardSummaryDto
        {
            TotalIncidents = summary.TotalIncidents,
            OpenIncidents = summary.OpenIncidents,
            CriticalIncidents = summary.CriticalIncidents,
            ResolvedThisWeek = summary.ResolvedThisWeek,
            UnassignedIncidents = summary.UnassignedIncidents,
            AssignedToMeIncidents = summary.AssignedToMeIncidents,
            Severity = summary.Severity.Select(item => new DashboardCountItemDto
            {
                Label = item.Label,
                Count = item.Count
            }).ToList(),
            Status = summary.Status.Select(item => new DashboardCountItemDto
            {
                Label = item.Label,
                Count = item.Count
            }).ToList(),
            Trend = summary.Trend.Select(item => new DashboardCountItemDto
            {
                Label = item.Label,
                Count = item.Count
            }).ToList()
        };
    }
}