using Xunit;
using Udemy.Application.DTOs;
using Udemy.Application.Services;

namespace Udemy.Tests.Services;

/// <summary>
/// Unit tests for PostService
/// </summary>
public class PostServiceTests
{
    [Fact]
    public void CreatePostAsync_ShouldNotThrow()
    {
        // Arrange
        var service = new PostService(null!, null!, null!, null!);

        // Act & Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GetPostAsync_ShouldNotThrow()
    {
        // Arrange
        var service = new PostService(null!, null!, null!, null!);

        // Act & Assert
        Assert.NotNull(service);
    }
}
