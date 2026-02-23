using IncidentFlow.Application.Features.Incidents.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Incidents;

public class UpdateIncidentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenIncidentMissing_ReturnsNull()
    {
        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();
        incidentRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Incident?)null);

        var handler = new UpdateIncidentCommandHandler(incidentRepository.Object, logRepository.Object);

        var result = await handler.Handle(new UpdateIncidentCommand { Id = Guid.NewGuid(), PerformedByUserId = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
        incidentRepository.Verify(x => x.UpdateAsync(It.IsAny<Incident>(), It.IsAny<CancellationToken>()), Times.Never);
        logRepository.Verify(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EnforcesBusinessRules_AndWritesOnlyChangedLogs()
    {
        var incident = new Incident("Initial", "Initial desc", SeverityLevel.Low, Guid.NewGuid())
        {
            Status = IncidentStatus.Open,
            AssignedTo = Guid.NewGuid()
        };

        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        incidentRepository
            .Setup(x => x.GetByIdAsync(incident.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(incident);
        incidentRepository
            .Setup(x => x.UpdateAsync(incident, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var capturedLogs = new List<IncidentLog>();
        logRepository
            .Setup(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()))
            .Callback<IncidentLog, CancellationToken>((log, _) => capturedLogs.Add(log))
            .Returns(Task.CompletedTask);

        var oldUpdatedAt = incident.UpdatedAt;
        var newAssignee = Guid.NewGuid();

        var handler = new UpdateIncidentCommandHandler(incidentRepository.Object, logRepository.Object);
        var result = await handler.Handle(new UpdateIncidentCommand
        {
            Id = incident.Id,
            PerformedByUserId = Guid.NewGuid(),
            Title = " ",
            Description = "Updated desc",
            Status = IncidentStatus.InProgress,
            Severity = SeverityLevel.High,
            AssignedTo = newAssignee,
            ResolvedAt = DateTime.UtcNow
        }, CancellationToken.None);

        Assert.Same(incident, result);
        Assert.Equal("Initial", incident.Title);
        Assert.Equal("Updated desc", incident.Description);
        Assert.Equal(IncidentStatus.InProgress, incident.Status);
        Assert.Equal(SeverityLevel.High, incident.Severity);
        Assert.Equal(newAssignee, incident.AssignedTo);
        Assert.True(incident.UpdatedAt >= oldUpdatedAt);
        Assert.Equal(3, capturedLogs.Count);
        Assert.Contains(capturedLogs, x => x.Action == "Status change");
        Assert.Contains(capturedLogs, x => x.Action == "Severity change");
        Assert.Contains(capturedLogs, x => x.Action == "Assignment change");
    }

    [Fact]
    public async Task Handle_WhenValuesNotChanged_DoesNotWriteChangeLogs()
    {
        var assignee = Guid.NewGuid();
        var incident = new Incident("Initial", "Initial desc", SeverityLevel.Low, Guid.NewGuid())
        {
            Status = IncidentStatus.Open,
            AssignedTo = assignee
        };

        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        incidentRepository.Setup(x => x.GetByIdAsync(incident.Id, It.IsAny<CancellationToken>())).ReturnsAsync(incident);
        incidentRepository.Setup(x => x.UpdateAsync(incident, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new UpdateIncidentCommandHandler(incidentRepository.Object, logRepository.Object);

        await handler.Handle(new UpdateIncidentCommand
        {
            Id = incident.Id,
            PerformedByUserId = Guid.NewGuid(),
            Status = IncidentStatus.Open,
            Severity = SeverityLevel.Low,
            AssignedTo = assignee
        }, CancellationToken.None);

        logRepository.Verify(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()), Times.Never);
        incidentRepository.Verify(x => x.UpdateAsync(incident, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatesTitleAndResolvedAt_WithoutExtraLogs()
    {
        var incident = new Incident("Initial", "Initial desc", SeverityLevel.Low, Guid.NewGuid())
        {
            Status = IncidentStatus.Open
        };

        var resolvedAt = DateTime.UtcNow;
        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        incidentRepository.Setup(x => x.GetByIdAsync(incident.Id, It.IsAny<CancellationToken>())).ReturnsAsync(incident);
        incidentRepository.Setup(x => x.UpdateAsync(incident, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new UpdateIncidentCommandHandler(incidentRepository.Object, logRepository.Object);

        var result = await handler.Handle(new UpdateIncidentCommand
        {
            Id = incident.Id,
            PerformedByUserId = Guid.NewGuid(),
            Title = "Updated title",
            ResolvedAt = resolvedAt
        }, CancellationToken.None);

        Assert.Same(incident, result);
        Assert.Equal("Updated title", incident.Title);
        Assert.Equal(resolvedAt, incident.ResolvedAt);
        logRepository.Verify(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
