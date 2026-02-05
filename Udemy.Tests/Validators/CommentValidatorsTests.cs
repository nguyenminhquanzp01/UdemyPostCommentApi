using FluentValidation.TestHelper;
using Xunit;
using Udemy.Application.DTOs;
using Udemy.Application.Validators;

namespace Udemy.Tests.Validators;

/// <summary>
/// Unit tests for Comment validators
/// </summary>
public class CommentValidatorsTests
{
    #region CreateCommentRequestValidator Tests

    [Fact]
    public void CreateCommentRequestValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest
        {
            Content = "This is a valid comment with meaningful content.",
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCommentRequestValidator_WithValidReply_ShouldPass()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var parentId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            Content = "This is a valid reply to a comment.",
            ParentId = parentId
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCommentRequestValidator_WithEmptyContent_ShouldFail()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest
        {
            Content = "",
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .Only();
    }

    [Fact]
    public void CreateCommentRequestValidator_WithNullContent_ShouldFail()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest
        {
            Content = null!,
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreateCommentRequestValidator_WithWhitespaceOnlyContent_ShouldFail()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest
        {
            Content = "   ",
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreateCommentRequestValidator_WithContentExceeding2000Characters_ShouldFail()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var longContent = new string('a', 2001);
        var request = new CreateCommentRequest
        {
            Content = longContent,
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Comment must not exceed 2000 characters.");
    }

    [Fact]
    public void CreateCommentRequestValidator_WithContentExactly2000Characters_ShouldPass()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var content = new string('a', 2000);
        var request = new CreateCommentRequest
        {
            Content = content,
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCommentRequestValidator_WithSingleCharacterContent_ShouldPass()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest
        {
            Content = "a",
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCommentRequestValidator_WithSpecialCharacters_ShouldPass()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest
        {
            Content = "Great post! 👍 @user123 #udemy https://example.com",
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateCommentRequestValidator_WithMultilineContent_ShouldPass()
    {
        // Arrange
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest
        {
            Content = "This is a comment with\nmultiple lines\nand proper formatting.",
            ParentId = null
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region UpdateCommentRequestValidator Tests

    [Fact]
    public void UpdateCommentRequestValidator_WithValidInput_ShouldPass()
    {
        // Arrange
        var validator = new UpdateCommentRequestValidator();
        var request = new UpdateCommentRequest
        {
            Content = "Updated comment with meaningful content."
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCommentRequestValidator_WithEmptyContent_ShouldFail()
    {
        // Arrange
        var validator = new UpdateCommentRequestValidator();
        var request = new UpdateCommentRequest
        {
            Content = ""
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .Only();
    }

    [Fact]
    public void UpdateCommentRequestValidator_WithNullContent_ShouldFail()
    {
        // Arrange
        var validator = new UpdateCommentRequestValidator();
        var request = new UpdateCommentRequest
        {
            Content = null!
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void UpdateCommentRequestValidator_WithWhitespaceOnlyContent_ShouldFail()
    {
        // Arrange
        var validator = new UpdateCommentRequestValidator();
        var request = new UpdateCommentRequest
        {
            Content = "   \t   "
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void UpdateCommentRequestValidator_WithContentExceeding2000Characters_ShouldFail()
    {
        // Arrange
        var validator = new UpdateCommentRequestValidator();
        var longContent = new string('x', 2001);
        var request = new UpdateCommentRequest
        {
            Content = longContent
        };

        // Act & Assert
        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Comment must not exceed 2000 characters.");
    }

    [Fact]
    public void UpdateCommentRequestValidator_WithContentExactly2000Characters_ShouldPass()
    {
        // Arrange
        var validator = new UpdateCommentRequestValidator();
        var content = new string('x', 2000);
        var request = new UpdateCommentRequest
        {
            Content = content
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCommentRequestValidator_WithSpecialCharacters_ShouldPass()
    {
        // Arrange
        var validator = new UpdateCommentRequestValidator();
        var request = new UpdateCommentRequest
        {
            Content = "Updated! 💬 Check this out: <b>HTML</b> & \"quotes\""
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateCommentRequestValidator_WithMinimalContent_ShouldPass()
    {
        // Arrange
        var validator = new UpdateCommentRequestValidator();
        var request = new UpdateCommentRequest
        {
            Content = "x"
        };

        // Act & Assert
        validator.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}
