using Xunit;
using Udemy.Application.DTOs;
using Udemy.Application.Validators;

namespace Udemy.Tests.Validators;

/// <summary>
/// Unit tests for Auth validators
/// </summary>
public class AuthValidatorsTests
{
    [Fact]
    public void RegisterRequestValidator_ShouldValidate()
    {
        // Arrange
        var validator = new RegisterRequestValidator();

        // Act & Assert
        Assert.NotNull(validator);
    }

    [Fact]
    public void LoginRequestValidator_ShouldValidate()
    {
        // Arrange
        var validator = new LoginRequestValidator();

        // Act & Assert
        Assert.NotNull(validator);
    }
}
