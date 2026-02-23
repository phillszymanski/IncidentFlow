using IncidentFlow.Application.Features.Comments.Queries;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Comments;

public class CommentQueriesHandlerTests
{
    [Fact]
    public async Task GetCommentById_ReturnsRepositoryResult()
    {
        var comment = new Comment { Content = "c" };
        var repository = new Mock<ICommentRepository>();
        repository.Setup(x => x.GetByIdAsync(comment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        var handler = new GetCommentByIdQueryHandler(repository.Object);

        var result = await handler.Handle(new GetCommentByIdQuery(comment.Id), CancellationToken.None);

        Assert.Same(comment, result);
    }

    [Fact]
    public async Task GetAllComments_ReturnsRepositoryList()
    {
        var comments = new List<Comment> { new(), new() };
        var repository = new Mock<ICommentRepository>();
        repository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(comments);

        var handler = new GetAllCommentsQueryHandler(repository.Object);

        var result = await handler.Handle(new GetAllCommentsQuery(), CancellationToken.None);

        Assert.Same(comments, result);
    }

    [Fact]
    public async Task GetCommentsByIncidentId_FiltersByIncident()
    {
        var incidentId = Guid.NewGuid();
        var comments = new List<Comment> { new() { IncidentId = incidentId } };
        var repository = new Mock<ICommentRepository>();
        repository.Setup(x => x.GetByIncidentIdAsync(incidentId, It.IsAny<CancellationToken>())).ReturnsAsync(comments);

        var handler = new GetCommentsByIncidentIdQueryHandler(repository.Object);

        var result = await handler.Handle(new GetCommentsByIncidentIdQuery(incidentId), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(incidentId, result[0].IncidentId);
    }
}
