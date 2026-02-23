using IncidentFlow.Application.Features.Comments.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Comments;

public class CreateCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesAndPersistsComment()
    {
        var repository = new Mock<ICommentRepository>();
        Comment? added = null;
        repository
            .Setup(x => x.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Callback<Comment, CancellationToken>((comment, _) => added = comment)
            .Returns(Task.CompletedTask);

        var handler = new CreateCommentCommandHandler(repository.Object);
        var command = new CreateCommentCommand
        {
            IncidentId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid(),
            Content = "hello"
        };

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(added);
        Assert.Equal(command.IncidentId, added!.IncidentId);
        Assert.Equal(command.CreatedByUserId, added.CreatedByUserId);
        Assert.Equal("hello", added.Content);
        Assert.InRange(added.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        repository.Verify(x => x.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_BubblesException()
    {
        var repository = new Mock<ICommentRepository>();
        repository
            .Setup(x => x.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db fail"));

        var handler = new CreateCommentCommandHandler(repository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreateCommentCommand(), CancellationToken.None));
    }
}
