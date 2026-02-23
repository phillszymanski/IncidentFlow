using IncidentFlow.Application.Features.Comments.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Comments;

public class UpdateCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCommentMissing_ReturnsNull()
    {
        var repository = new Mock<ICommentRepository>();
        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment?)null);

        var handler = new UpdateCommentCommandHandler(repository.Object);

        var result = await handler.Handle(new UpdateCommentCommand { Id = Guid.NewGuid(), Content = "new" }, CancellationToken.None);

        Assert.Null(result);
        repository.Verify(x => x.UpdateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhitespaceContent_DoesNotOverwrite()
    {
        var comment = new Comment { Content = "existing" };
        var repository = new Mock<ICommentRepository>();
        repository
            .Setup(x => x.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        repository
            .Setup(x => x.UpdateAsync(comment, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateCommentCommandHandler(repository.Object);

        var result = await handler.Handle(new UpdateCommentCommand { Id = comment.Id, Content = "   " }, CancellationToken.None);

        Assert.Same(comment, result);
        Assert.Equal("existing", comment.Content);
        repository.Verify(x => x.UpdateAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
    }
}
