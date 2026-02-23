using IncidentFlow.Application.Features.Incidents.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Incidents;

public class DeleteIncidentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenIncidentMissing_NoOp()
    {
        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        incidentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Incident?)null);

        var handler = new DeleteIncidentCommandHandler(incidentRepository.Object, logRepository.Object);

        await handler.Handle(new DeleteIncidentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        incidentRepository.Verify(x => x.UpdateAsync(It.IsAny<Incident>(), It.IsAny<CancellationToken>()), Times.Never);
        logRepository.Verify(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SoftDeletesAndWritesAuditLog()
    {
        var incident = new Incident("t", "d", SeverityLevel.Medium, Guid.NewGuid());

        var incidentRepository = new Mock<IIncidentRepository>();
        var logRepository = new Mock<IIncidentLogRepository>();

        incidentRepository.Setup(x => x.GetByIdAsync(incident.Id, It.IsAny<CancellationToken>())).ReturnsAsync(incident);
        incidentRepository.Setup(x => x.UpdateAsync(incident, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        logRepository.Setup(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var performedBy = Guid.NewGuid();
        var command = new DeleteIncidentCommand(incident.Id, performedBy);
        var handler = new DeleteIncidentCommandHandler(incidentRepository.Object, logRepository.Object);

        await handler.Handle(command, CancellationToken.None);

        Assert.True(incident.IsDeleted);
        Assert.NotNull(incident.DeletedAt);
        incidentRepository.Verify(x => x.UpdateAsync(incident, It.IsAny<CancellationToken>()), Times.Once);
        logRepository.Verify(x => x.AddAsync(It.Is<IncidentLog>(log =>
            log.IncidentId == incident.Id &&
            log.Action == "Delete (soft)" &&
            log.PerformedByUserId == performedBy), It.IsAny<CancellationToken>()), Times.Once);
    }
}
