using IncidentFlow.Application.Features.Incidents.Queries;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Incidents;

public class IncidentQueriesHandlerTests
{
    [Fact]
    public async Task GetById_ReturnsRepositoryResult()
    {
        var incident = new Incident("t", "d", SeverityLevel.Low, Guid.NewGuid());
        var incidentRepository = new Mock<IIncidentRepository>();
        incidentRepository.Setup(x => x.GetByIdAsync(incident.Id, It.IsAny<CancellationToken>())).ReturnsAsync(incident);

        var handler = new GetIncidentByIdQueryHandler(incidentRepository.Object);

        var result = await handler.Handle(new GetIncidentByIdQuery(incident.Id), CancellationToken.None);

        Assert.Same(incident, result);
    }

    [Fact]
    public async Task GetAll_NormalizesPagingBeforeRepositoryCall()
    {
        var incidentRepository = new Mock<IIncidentRepository>();
        incidentRepository
            .Setup(x => x.GetPagedAsync(1, 1, IncidentListFilterType.Open, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Incident>(), 0));

        var handler = new GetAllIncidentsQueryHandler(incidentRepository.Object);

        var result = await handler.Handle(new GetAllIncidentsQuery(0, 0, IncidentListFilterType.Open, null), CancellationToken.None);

        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        incidentRepository.Verify(x => x.GetPagedAsync(1, 1, IncidentListFilterType.Open, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dashboard_ComputesSummaryAndAssignedToMeMetrics()
    {
        var me = Guid.NewGuid();
        var incidents = new List<Incident>
        {
            new("a", "a", SeverityLevel.Critical, Guid.NewGuid()) { AssignedTo = me, Status = IncidentStatus.Open },
            new("b", "b", SeverityLevel.Low, Guid.NewGuid()) { AssignedTo = null, Status = IncidentStatus.Resolved, ResolvedAt = DateTime.UtcNow }
        };

        var incidentRepository = new Mock<IIncidentRepository>();
        incidentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(incidents);

        var handler = new GetIncidentDashboardSummaryQueryHandler(incidentRepository.Object);

        var result = await handler.Handle(new GetIncidentDashboardSummaryQuery(me), CancellationToken.None);

        Assert.Equal(2, result.TotalIncidents);
        Assert.Equal(1, result.OpenIncidents);
        Assert.Equal(1, result.CriticalIncidents);
        Assert.Equal(1, result.UnassignedIncidents);
        Assert.Equal(1, result.AssignedToMeIncidents);
        Assert.Equal(4, result.Severity.Count);
        Assert.Equal(4, result.Status.Count);
        Assert.Equal(7, result.Trend.Count);
    }

    [Fact]
    public async Task Dashboard_WhenCurrentUserMissing_AssignedToMeIsZero()
    {
        var incidents = new List<Incident>
        {
            new("a", "a", SeverityLevel.Low, Guid.NewGuid()) { AssignedTo = Guid.NewGuid() }
        };

        var incidentRepository = new Mock<IIncidentRepository>();
        incidentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(incidents);

        var handler = new GetIncidentDashboardSummaryQueryHandler(incidentRepository.Object);

        var result = await handler.Handle(new GetIncidentDashboardSummaryQuery(null), CancellationToken.None);

        Assert.Equal(0, result.AssignedToMeIncidents);
    }
}
