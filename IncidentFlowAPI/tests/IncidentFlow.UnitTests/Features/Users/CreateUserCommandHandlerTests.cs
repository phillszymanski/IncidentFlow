using IncidentFlow.Application.Features.Users.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Users;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_MapsUserAndCallsRepository()
    {
        var repository = new Mock<IUserRepository>();
        User? addedUser = null;
        repository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => addedUser = user)
            .Returns(Task.CompletedTask);

        var handler = new CreateUserCommandHandler(repository.Object);
        var command = new CreateUserCommand
        {
            Username = "new-user",
            Email = "new@test.local",
            FullName = "New User",
            Role = "User",
            PasswordHash = "hash"
        };

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.NotNull(addedUser);
        Assert.Equal("new-user", addedUser!.Username);
        Assert.Equal("new@test.local", addedUser.Email);
        Assert.Equal("New User", addedUser.FullName);
        Assert.Equal("User", addedUser.Role);
        Assert.Equal("hash", addedUser.PasswordHash);
        Assert.InRange(addedUser.CreatedAt, DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        repository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_BubblesException()
    {
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db fail"));

        var handler = new CreateUserCommandHandler(repository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new CreateUserCommand(), CancellationToken.None));
    }
}
