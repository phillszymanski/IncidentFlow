using IncidentFlow.Application.Features.Users.Queries;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Users;

public class GetAllUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            new() { Username = "a" },
            new() { Username = "b" }
        };

        var repository = new Mock<IUserRepository>();
        repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var handler = new GetAllUsersQueryHandler(repository.Object);

        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Same(users, result);
    }
}
