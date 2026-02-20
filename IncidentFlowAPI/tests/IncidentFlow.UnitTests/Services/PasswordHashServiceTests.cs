using IncidentFlow.API.Services;

namespace IncidentFlow.UnitTests.Services;

public class PasswordHashServiceTests
{
    private readonly PasswordHashService _service = new();

    [Fact]
    public void HashPassword_ProducesVersionedHash_And_VerifiesSuccessfully()
    {
        var password = "S3cureP@ssword!";

        var hash = _service.HashPassword(password);

        Assert.StartsWith("v1.", hash);
        Assert.True(_service.VerifyPassword(password, hash));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForWrongPassword()
    {
        var hash = _service.HashPassword("CorrectPassword123!");

        var isValid = _service.VerifyPassword("WrongPassword123!", hash);

        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForMalformedHash()
    {
        var isValid = _service.VerifyPassword("any", "not-a-valid-hash-format");

        Assert.False(isValid);
    }
}
