using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Queries;

public sealed record GetIncidentDashboardSummaryQuery(Guid? CurrentUserId = null) : IRequest<IncidentDashboardSummaryResult>;

public sealed record IncidentDashboardSummaryResult(
    int TotalIncidents,
    int OpenIncidents,
    int CriticalIncidents,
    int ResolvedThisWeek,
    int UnassignedIncidents,
    int AssignedToMeIncidents,
    IReadOnlyList<DashboardCountItemResult> Severity,
    IReadOnlyList<DashboardCountItemResult> Status,
    IReadOnlyList<DashboardCountItemResult> Trend);

public sealed record DashboardCountItemResult(string Label, int Count);
