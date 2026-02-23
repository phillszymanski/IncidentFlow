using IncidentFlow.Application.Features.Users.Queries;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Users;

public class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsUserFromRepository()
    {
        var user = new User { Username = "u" };
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetUserByIdQueryHandler(repository.Object);

        var result = await handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.Same(user, result);
    }
}
