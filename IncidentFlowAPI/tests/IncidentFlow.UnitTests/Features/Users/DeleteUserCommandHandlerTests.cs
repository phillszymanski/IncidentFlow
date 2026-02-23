using IncidentFlow.Application.Features.Users.Commands;
using IncidentFlow.Application.Interfaces;
using Moq;

namespace IncidentFlow.UnitTests.Features.Users;

public class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesById()
    {
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DeleteUserCommandHandler(repository.Object);
        var command = new DeleteUserCommand(Guid.NewGuid());

        await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
