using IncidentFlow.Application.Features.Incidents.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Incidents;

public class RestoreIncidentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenMissingOrNotDeleted_ReturnsFalse()
    {
        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        incidentRepository
            .SetupSequence(x => x.GetByIdIncludingDeletedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Incident?)null)
            .ReturnsAsync(new Incident("t", "d", SeverityLevel.Low, Guid.NewGuid()) { IsDeleted = false });

        var handler = new RestoreIncidentCommandHandler(incidentRepository.Object, logRepository.Object);

        var missingResult = await handler.Handle(new RestoreIncidentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        var notDeletedResult = await handler.Handle(new RestoreIncidentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.False(missingResult);
        Assert.False(notDeletedResult);
        incidentRepository.Verify(x => x.UpdateAsync(It.IsAny<Incident>(), It.IsAny<CancellationToken>()), Times.Never);
        logRepository.Verify(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeletedIncident_RestoresAndWritesAuditLog()
    {
        var incident = new Incident("t", "d", SeverityLevel.Low, Guid.NewGuid())
        {
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddHours(-1)
        };

        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        incidentRepository.Setup(x => x.GetByIdIncludingDeletedAsync(incident.Id, It.IsAny<CancellationToken>())).ReturnsAsync(incident);
        incidentRepository.Setup(x => x.UpdateAsync(incident, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        logRepository.Setup(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new RestoreIncidentCommand(incident.Id, Guid.NewGuid());
        var handler = new RestoreIncidentCommandHandler(incidentRepository.Object, logRepository.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        Assert.False(incident.IsDeleted);
        Assert.Null(incident.DeletedAt);
        logRepository.Verify(x => x.AddAsync(It.Is<IncidentLog>(log => log.Action == "Restore"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
