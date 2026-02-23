using IncidentFlow.Application.Features.IncidentLogs.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.IncidentLogs;

public class CreateIncidentLogCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesAndPersistsIncidentLog()
    {
        var repository = new Mock<IIncidentLogRepository>();
        IncidentLog? added = null;
        repository
            .Setup(x => x.AddAsync(It.IsAny<IncidentLog>(), It.IsAny<CancellationToken>()))
            .Callback<IncidentLog, CancellationToken>((log, _) => added = log)
            .Returns(Task.CompletedTask);

        var handler = new CreateIncidentLogCommandHandler(repository.Object);
        var command = new CreateIncidentLogCommand
        {
            IncidentId = Guid.NewGuid(),
            Action = "Action",
            Details = "Details",
            PerformedByUserId = Guid.NewGuid()
        };

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(command.IncidentId, added!.IncidentId);
        Assert.Equal(command.Action, added.Action);
        Assert.Equal(command.Details, added.Details);
        Assert.Equal(command.PerformedByUserId, added.PerformedByUserId);
        Assert.InRange(added.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
    }
}
