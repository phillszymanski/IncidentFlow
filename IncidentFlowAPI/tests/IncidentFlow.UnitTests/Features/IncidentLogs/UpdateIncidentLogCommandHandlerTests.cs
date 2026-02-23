using IncidentFlow.Application.Features.IncidentLogs.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.IncidentLogs;

public class UpdateIncidentLogCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenMissing_ReturnsNull()
    {
        var repository = new Mock<IIncidentLogRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((IncidentLog?)null);

        var handler = new UpdateIncidentLogCommandHandler(repository.Object);

        var result = await handler.Handle(new UpdateIncidentLogCommand { Id = Guid.NewGuid(), Action = "new" }, CancellationToken.None);

        Assert.Null(result);
        repository.Verify(x => x.UpdateAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OnlyAppliesProvidedValues()
    {
        var existing = new IncidentLog
        {
            IncidentId = Guid.NewGuid(),
            Action = "old-action",
            Details = "old-details",
            PerformedByUserId = Guid.NewGuid()
        };

        var repository = new Mock<IIncidentLogRepository>();
        repository.Setup(x => x.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        repository.Setup(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new UpdateIncidentLogCommandHandler(repository.Object);
        var command = new UpdateIncidentLogCommand
        {
            Id = existing.Id,
            Action = " ",
            Details = "updated",
            IncidentId = null,
            PerformedByUserId = Guid.NewGuid()
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Same(existing, result);
        Assert.Equal("old-action", existing.Action);
        Assert.Equal("updated", existing.Details);
        Assert.Equal(command.PerformedByUserId, existing.PerformedByUserId);
        repository.Verify(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatesIncidentIdAndAction_WhenProvided()
    {
        var existing = new IncidentLog
        {
            IncidentId = Guid.NewGuid(),
            Action = "old-action",
            Details = "old-details",
            PerformedByUserId = Guid.NewGuid()
        };

        var newIncidentId = Guid.NewGuid();

        var repository = new Mock<IIncidentLogRepository>();
        repository.Setup(x => x.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        repository.Setup(x => x.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new UpdateIncidentLogCommandHandler(repository.Object);

        await handler.Handle(new UpdateIncidentLogCommand
        {
            Id = existing.Id,
            IncidentId = newIncidentId,
            Action = "new-action"
        }, CancellationToken.None);

        Assert.Equal(newIncidentId, existing.IncidentId);
        Assert.Equal("new-action", existing.Action);
    }
}
