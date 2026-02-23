using IncidentFlow.Application.Features.Comments.Commands;
using IncidentFlow.Application.Interfaces;
using Moq;

namespace IncidentFlow.UnitTests.Features.Comments;

public class DeleteCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesById()
    {
        var repository = new Mock<ICommentRepository>();
        repository.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new DeleteCommentCommandHandler(repository.Object);
        var command = new DeleteCommentCommand(Guid.NewGuid());

        await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
