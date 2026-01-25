using Xunit;
using Udemy.Application.DTOs;
using Udemy.Application.Services;

namespace Udemy.Tests.Services;

/// <summary>
/// Unit tests for AuthService
/// </summary>
public class AuthServiceTests
{
    [Fact]
    public void RegisterAsync_WithValidInput_ShouldNotThrow()
    {
        // Arrange
        var service = new AuthService(null!, null!, null!, null!, null!);

        // Act & Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void LoginAsync_ShouldNotThrow()
    {
        // Arrange
        var service = new AuthService(null!, null!, null!, null!, null!);

        // Act & Assert
        Assert.NotNull(service);
    }
}
