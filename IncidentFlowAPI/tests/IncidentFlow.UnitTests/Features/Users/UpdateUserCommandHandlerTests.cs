using IncidentFlow.Application.Features.Users.Commands;
using IncidentFlow.Application.Interfaces;
using IncidentFlow.Domain.Entities;
using Moq;

namespace IncidentFlow.UnitTests.Features.Users;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserMissing_ReturnsNullWithoutUpdate()
    {
        var repository = new Mock<IUserRepository>();
        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new UpdateUserCommandHandler(repository.Object);

        var result = await handler.Handle(new UpdateUserCommand { Id = Guid.NewGuid(), Username = "updated" }, CancellationToken.None);

        Assert.Null(result);
        repository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesOnlyNonWhitespaceFields()
    {
        var user = new User
        {
            Username = "old-user",
            Email = "old@test.local",
            FullName = "Old Name",
            Role = "User",
            PasswordHash = "old-hash"
        };

        var repository = new Mock<IUserRepository>();
        repository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        repository
            .Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateUserCommandHandler(repository.Object);
        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Username = "  ",
            Email = "new@test.local",
            FullName = "New Name",
            Role = null,
            PasswordHash = "new-hash"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Same(user, result);
        Assert.Equal("old-user", user.Username);
        Assert.Equal("new@test.local", user.Email);
        Assert.Equal("New Name", user.FullName);
        Assert.Equal("User", user.Role);
        Assert.Equal("new-hash", user.PasswordHash);
        repository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdatesUsernameAndRole_WhenProvided()
    {
        var user = new User
        {
            Username = "old-user",
            Email = "old@test.local",
            FullName = "Old Name",
            Role = "User",
            PasswordHash = "old-hash"
        };

        var repository = new Mock<IUserRepository>();
        repository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var handler = new UpdateUserCommandHandler(repository.Object);

        await handler.Handle(new UpdateUserCommand
        {
            Id = user.Id,
            Username = "new-user",
            Role = "Manager"
        }, CancellationToken.None);

        Assert.Equal("new-user", user.Username);
        Assert.Equal("Manager", user.Role);
    }
}
