using IncidentFlow.Application.Features.Incidents.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Incidents;

public class CreateIncidentCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesIncidentWithDomainEvent_AndAuditLog()
    {
        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        Incident? addedIncident = null;
        IncidentLog? addedLog = null;

        incidentRepository
            .Setup(x => x.AddAsync(It.IsAny<Incident>(), It.IsAny<CancellationToken>()))
            .Callback<Incident, CancellationToken>((incident, _) => addedIncident = incident)
            .Returns(Task.CompletedTask);

        logRepository
            .Setup(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()))
            .Callback<IncidentLog, CancellationToken>((log, _) => addedLog = log)
            .Returns(Task.CompletedTask);

        var handler = new CreateIncidentCommandHandler(incidentRepository.Object, logRepository.Object);
        var command = new CreateIncidentCommand
        {
            Title = "Database outage",
            Description = "Primary DB unreachable",
            Severity = SeverityLevel.Critical,
            CreatedBy = Guid.NewGuid(),
            AssignedTo = Guid.NewGuid()
        };

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(addedIncident);
        Assert.Equal("Database outage", addedIncident!.Title);
        Assert.Equal("Primary DB unreachable", addedIncident.Description);
        Assert.Equal(SeverityLevel.Critical, addedIncident.Severity);
        Assert.Equal(command.CreatedBy, addedIncident.CreatedBy);
        Assert.Equal(command.AssignedTo, addedIncident.AssignedTo);
        Assert.Contains(addedIncident.DomainEvents, e => e is IncidentCreatedEvent);

        Assert.NotNull(addedLog);
        Assert.Equal(addedIncident.Id, addedLog!.IncidentId);
        Assert.Equal("Create", addedLog.Action);
        Assert.Contains("assignment", addedLog.Details, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(command.CreatedBy, addedLog.PerformedByUserId);

        incidentRepository.Verify(x => x.AddAsync(It.IsAny<Incident>(), It.IsAny<CancellationToken>()), Times.Once);
        logRepository.Verify(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLogPersistenceFails_Throws()
    {
        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        incidentRepository.Setup(x => x.AddAsync(It.IsAny<Incident>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        logRepository
            .Setup(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("log failure"));

        var handler = new CreateIncidentCommandHandler(incidentRepository.Object, logRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreateIncidentCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNoAssignee_LogsCreationWithoutAssignmentDetails()
    {
        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        IncidentLog? addedLog = null;

        incidentRepository
            .Setup(x => x.AddAsync(It.IsAny<Incident>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        logRepository
            .Setup(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()))
            .Callback<IncidentLog, CancellationToken>((log, _) => addedLog = log)
            .Returns(Task.CompletedTask);

        var handler = new CreateIncidentCommandHandler(incidentRepository.Object, logRepository.Object);
        var command = new CreateIncidentCommand
        {
            Title = "No assignee",
            Description = "No assignment branch",
            Severity = SeverityLevel.Low,
            CreatedBy = Guid.NewGuid(),
            AssignedTo = null
        };

        await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(addedLog);
        Assert.DoesNotContain("assignment", addedLog!.Details, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(".", addedLog.Details, StringComparison.Ordinal);
    }
}
