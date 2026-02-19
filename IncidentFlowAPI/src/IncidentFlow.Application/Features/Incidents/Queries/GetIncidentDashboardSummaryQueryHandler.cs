using IncidentFlow.Application.Interfaces;
using MediatR;

namespace IncidentFlow.Application.Features.Incidents.Queries;

public sealed class GetIncidentDashboardSummaryQueryHandler : IRequestHandler<GetIncidentDashboardSummaryQuery, IncidentDashboardSummaryResult>
{
    private readonly IIncidentRepository _repository;

    public GetIncidentDashboardSummaryQueryHandler(IIncidentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IncidentDashboardSummaryResult> Handle(GetIncidentDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var incidents = await _repository.GetAllAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var startOfWeek = GetStartOfWeekUtc(now);

        var totalIncidents = incidents.Count;
        var openIncidents = incidents.Count(i => i.Status == IncidentStatus.Open);
        var criticalIncidents = incidents.Count(i => i.Severity == SeverityLevel.Critical);
        var resolvedThisWeek = incidents.Count(i => i.ResolvedAt.HasValue && i.ResolvedAt.Value >= startOfWeek);
        var unassignedIncidents = incidents.Count(i => !i.AssignedTo.HasValue);
        var assignedToMeIncidents = request.CurrentUserId.HasValue
            ? incidents.Count(i => i.AssignedTo == request.CurrentUserId)
            : 0;

        var severity = new[]
        {
            new DashboardCountItemResult("Low", incidents.Count(i => i.Severity == SeverityLevel.Low)),
            new DashboardCountItemResult("Medium", incidents.Count(i => i.Severity == SeverityLevel.Medium)),
            new DashboardCountItemResult("High", incidents.Count(i => i.Severity == SeverityLevel.High)),
            new DashboardCountItemResult("Critical", incidents.Count(i => i.Severity == SeverityLevel.Critical))
        };

        var status = new[]
        {
            new DashboardCountItemResult("Open", incidents.Count(i => i.Status == IncidentStatus.Open)),
            new DashboardCountItemResult("InProgress", incidents.Count(i => i.Status == IncidentStatus.InProgress)),
            new DashboardCountItemResult("Resolved", incidents.Count(i => i.Status == IncidentStatus.Resolved)),
            new DashboardCountItemResult("Closed", incidents.Count(i => i.Status == IncidentStatus.Closed))
        };

        var trend = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var day = now.Date.AddDays(-(6 - offset));
                var nextDay = day.AddDays(1);
                var count = incidents.Count(i => i.CreatedAt >= day && i.CreatedAt < nextDay);
                return new DashboardCountItemResult(day.ToString("ddd"), count);
            })
            .ToList();

        return new IncidentDashboardSummaryResult(
            totalIncidents,
            openIncidents,
            criticalIncidents,
            resolvedThisWeek,
            unassignedIncidents,
            assignedToMeIncidents,
            severity,
            status,
            trend);
    }

    private static DateTime GetStartOfWeekUtc(DateTime currentUtc)
    {
        var day = (int)currentUtc.DayOfWeek;
        var diff = day == 0 ? -6 : 1 - day;
        return currentUtc.Date.AddDays(diff);
    }
}
