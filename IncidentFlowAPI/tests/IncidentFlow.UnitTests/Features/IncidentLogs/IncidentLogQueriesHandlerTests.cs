using IncidentFlow.Application.Features.IncidentLogs.Queries;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.IncidentLogs;

public class IncidentLogQueriesHandlerTests
{
    [Fact]
    public async Task GetById_ReturnsRepositoryResult()
    {
        var incidentLog = new IncidentLog { Action = "a" };
        var repository = new Mock<IIncidentLogRepository>();
        repository.Setup(x => x.GetByIdAsync(incidentLog.Id, It.IsAny<CancellationToken>())).ReturnsAsync(incidentLog);

        var handler = new GetIncidentLogByIdQueryHandler(repository.Object);

        var result = await handler.Handle(new GetIncidentLogByIdQuery(incidentLog.Id), CancellationToken.None);

        Assert.Same(incidentLog, result);
    }

    [Fact]
    public async Task GetAll_ReturnsRepositoryList()
    {
        var logs = new List<IncidentLog> { new(), new() };
        var repository = new Mock<IIncidentLogRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(logs);

        var handler = new GetAllIncidentLogsQueryHandler(repository.Object);

        var result = await handler.Handle(new GetAllIncidentLogsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIncidentId_ReturnsRepositoryList()
    {
        var incidentId = Guid.NewGuid();
        var logs = new List<IncidentLog> { new() { IncidentId = incidentId } };
        var repository = new Mock<IIncidentLogRepository>();
        repository.Setup(x => x.GetByIncidentIdAsync(incidentId, It.IsAny<CancellationToken>())).ReturnsAsync(logs);

        var handler = new GetIncidentLogsByIncidentIdQueryHandler(repository.Object);

        var result = await handler.Handle(new GetIncidentLogsByIncidentIdQuery(incidentId), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(incidentId, result[0].IncidentId);
    }
}
