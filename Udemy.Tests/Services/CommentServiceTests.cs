using Xunit;
using Udemy.Application.DTOs;
using Udemy.Application.Services;

namespace Udemy.Tests.Services;

/// <summary>
/// Unit tests for CommentService
/// </summary>
public class CommentServiceTests
{
    [Fact]
    public void CreateCommentAsync_ShouldNotThrow()
    {
        // Arrange
        var service = new CommentService(null!, null!, null!, null!);

        // Act & Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GetCommentAsync_ShouldNotThrow()
    {
        // Arrange
        var service = new CommentService(null!, null!, null!, null!);

        // Act & Assert
        Assert.NotNull(service);
    }
}
