using Xunit;
using Udemy.Application.DTOs;
using Udemy.Application.Validators;

namespace Udemy.Tests.Validators;

/// <summary>
/// Unit tests for Post validators
/// </summary>
public class PostValidatorsTests
{
    [Fact]
    public void CreatePostRequestValidator_ShouldValidate()
    {
        // Arrange
        var validator = new CreatePostRequestValidator();

        // Act & Assert
        Assert.NotNull(validator);
    }

    [Fact]
    public void UpdatePostRequestValidator_ShouldValidate()
    {
        // Arrange
        var validator = new UpdatePostRequestValidator();

        // Act & Assert
        Assert.NotNull(validator);
    }
}
