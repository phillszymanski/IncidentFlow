using IncidentFlow.Application.Features.IncidentLogs.Commands;
using IncidentFlow.Application.Interfaces;
using Moq;

namespace IncidentFlow.UnitTests.Features.IncidentLogs;

public class DeleteIncidentLogCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesById()
    {
        var repository = new Mock<IIncidentLogRepository>();
        repository.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new DeleteIncidentLogCommandHandler(repository.Object);
        var command = new DeleteIncidentLogCommand(Guid.NewGuid());

        await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
