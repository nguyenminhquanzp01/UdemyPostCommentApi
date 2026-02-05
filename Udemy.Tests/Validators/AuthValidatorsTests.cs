using FluentValidation.TestHelper;
using Xunit;
using Udemy.Application.DTOs;
using Udemy.Application.Validators;

namespace Udemy.Tests.Validators;

/// <summary>
/// Unit tests for Auth validators
/// </summary>
public class AuthValidatorsTests
{
    #region RegisterRequestValidator Tests

    [Fact]
    public void RegisterRequestValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "validuser123",
            Email = "user@example.com",
            Name = "John Doe",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterRequestValidator_WithEmptyUsername_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "",
            Email = "user@example.com",
            Name = "John Doe",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Username)
            .Only();
    }

    [Fact]
    public void RegisterRequestValidator_WithShortUsername_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "ab",
            Email = "user@example.com",
            Name = "John Doe",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Username)
            .Only();
    }

    [Fact]
    public void RegisterRequestValidator_WithInvalidCharactersInUsername_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "invalid@user#",
            Email = "user@example.com",
            Name = "John Doe",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Username)
            .Only();
    }

    [Fact]
    public void RegisterRequestValidator_WithEmptyEmail_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "",
            Name = "John Doe",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Email)
            .Only();
    }

    [Fact]
    public void RegisterRequestValidator_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "invalid-email",
            Name = "John Doe",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Email)
            .Only();
    }

    [Fact]
    public void RegisterRequestValidator_WithEmptyName_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "user@example.com",
            Name = "",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Name)
            .Only();
    }

    [Fact]
    public void RegisterRequestValidator_WithShortName_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "user@example.com",
            Name = "J",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Name)
            .Only();
    }

    [Fact]
    public void RegisterRequestValidator_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "user@example.com",
            Name = "John Doe",
            Password = ""
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Password)
            .Only();
    }

    [Fact]
    public void RegisterRequestValidator_WithShortPassword_ShouldFail()
    {
        // Arrange
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest
        {
            Username = "validuser",
            Email = "user@example.com",
            Name = "John Doe",
            Password = "Pass1"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Password)
            .Only();
    }

    #endregion

    #region LoginRequestValidator Tests

    [Fact]
    public void LoginRequestValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Username = "validuser",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LoginRequestValidator_WithEmptyUsername_ShouldFail()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Username = "",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username is required.");
    }

    [Fact]
    public void LoginRequestValidator_WithShortUsername_ShouldFail()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Username = "ab",
            Password = "Password123!"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Username)
            .WithErrorMessage("Username must be at least 3 characters.");
    }

    [Fact]
    public void LoginRequestValidator_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Username = "validuser",
            Password = ""
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required.");
    }

    [Fact]
    public void LoginRequestValidator_WithShortPassword_ShouldFail()
    {
        // Arrange
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Username = "validuser",
            Password = "Pass1"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters.");
    }

    #endregion
}
