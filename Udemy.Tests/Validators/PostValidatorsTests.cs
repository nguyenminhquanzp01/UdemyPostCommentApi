using FluentValidation.TestHelper;
using Xunit;
using Udemy.Application.DTOs;
using Udemy.Application.Validators;

namespace Udemy.Tests.Validators;

/// <summary>
/// Unit tests for Post validators
/// </summary>
public class PostValidatorsTests
{
    #region CreatePostRequestValidator Tests

    [Fact]
    public void CreatePostRequestValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new CreatePostRequestValidator();
        var request = new CreatePostRequest
        {
            Title = "Valid Post Title",
            Content = "This is a valid post content with enough characters."
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePostRequestValidator_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var validator = new CreatePostRequestValidator();
        var request = new CreatePostRequest
        {
            Title = "",
            Content = "This is a valid post content with enough characters."
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void CreatePostRequestValidator_WithShortTitle_ShouldFail()
    {
        // Arrange
        var validator = new CreatePostRequestValidator();
        var request = new CreatePostRequest
        {
            Title = "Test",
            Content = "This is a valid post content with enough characters."
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must be at least 5 characters.");
    }

    [Fact]
    public void CreatePostRequestValidator_WithLongTitle_ShouldFail()
    {
        // Arrange
        var validator = new CreatePostRequestValidator();
        var request = new CreatePostRequest
        {
            Title = new string('a', 501),
            Content = "This is a valid post content with enough characters."
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 500 characters.");
    }

    [Fact]
    public void CreatePostRequestValidator_WithEmptyContent_ShouldFail()
    {
        // Arrange
        var validator = new CreatePostRequestValidator();
        var request = new CreatePostRequest
        {
            Title = "Valid Title",
            Content = ""
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content is required.");
    }

    [Fact]
    public void CreatePostRequestValidator_WithShortContent_ShouldFail()
    {
        // Arrange
        var validator = new CreatePostRequestValidator();
        var request = new CreatePostRequest
        {
            Title = "Valid Title",
            Content = "Short"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content must be at least 10 characters.");
    }

    [Fact]
    public void CreatePostRequestValidator_WithMinimumValidLength_ShouldPass()
    {
        // Arrange
        var validator = new CreatePostRequestValidator();
        var request = new CreatePostRequest
        {
            Title = "Abcde",  // Exactly 5 characters
            Content = "1234567890"  // Exactly 10 characters
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region UpdatePostRequestValidator Tests

    [Fact]
    public void UpdatePostRequestValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new UpdatePostRequestValidator();
        var request = new UpdatePostRequest
        {
            Title = "Updated Post Title",
            Content = "This is an updated post content with enough characters."
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdatePostRequestValidator_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var validator = new UpdatePostRequestValidator();
        var request = new UpdatePostRequest
        {
            Title = "",
            Content = "This is an updated post content with enough characters."
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void UpdatePostRequestValidator_WithShortTitle_ShouldFail()
    {
        // Arrange
        var validator = new UpdatePostRequestValidator();
        var request = new UpdatePostRequest
        {
            Title = "Updt",
            Content = "This is an updated post content with enough characters."
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must be at least 5 characters.");
    }

    [Fact]
    public void UpdatePostRequestValidator_WithLongTitle_ShouldFail()
    {
        // Arrange
        var validator = new UpdatePostRequestValidator();
        var request = new UpdatePostRequest
        {
            Title = new string('x', 501),
            Content = "This is an updated post content with enough characters."
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 500 characters.");
    }

    [Fact]
    public void UpdatePostRequestValidator_WithEmptyContent_ShouldFail()
    {
        // Arrange
        var validator = new UpdatePostRequestValidator();
        var request = new UpdatePostRequest
        {
            Title = "Updated Title",
            Content = ""
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content is required.");
    }

    [Fact]
    public void UpdatePostRequestValidator_WithShortContent_ShouldFail()
    {
        // Arrange
        var validator = new UpdatePostRequestValidator();
        var request = new UpdatePostRequest
        {
            Title = "Updated Title",
            Content = "Short"
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Content must be at least 10 characters.");
    }

    #endregion
}
