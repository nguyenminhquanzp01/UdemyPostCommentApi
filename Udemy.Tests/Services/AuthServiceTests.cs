using Moq;
using Xunit;
using AutoMapper;
using FluentAssertions;
using Udemy.Application.DTOs;
using Udemy.Application.Services;
using Udemy.Application.Repositories;
using Udemy.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Udemy.Tests.Services;

/// <summary>
/// Unit tests for AuthService using Repository pattern
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<AuthService>> _mockLogger;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AuthService>>();
    }

    private AuthService CreateAuthService()
    {
        return new AuthService(
            _mockUserRepository.Object,
            _mockTokenService.Object,
            _mockCacheService.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Name = "New User",
            Password = "Password123!"
        };

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            Role = "User",
            IsActive = true
        };

        var userDto = new UserDto 
        { 
            Id = newUser.Id, 
            Username = newUser.Username, 
            Email = newUser.Email,
            Name = newUser.Name,
            Role = newUser.Role
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUser);
        _mockTokenService.Setup(x => x.GenerateAccessToken(newUser.Id, newUser.Username, newUser.Role))
            .Returns("access_token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");
        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);
        _mockUserRepository.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var authService = CreateAuthService();

        // Act
        var result = await authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_UserExists_ShouldThrow()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Name = "Existing User",
            Password = "Password123!"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Email = "existing@example.com",
            Name = "Existing User",
            Role = "User",
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var authService = CreateAuthService();

        // Act
        Func<Task> act = () => authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Username already exists.");
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_EmailExists_ShouldThrow()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "existing@example.com",
            Name = "New User",
            Password = "Password123!"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "otheruser",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Email = "existing@example.com",
            Name = "Existing User",
            Role = "User",
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);
        _mockUserRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var authService = CreateAuthService();

        // Act
        Func<Task> act = () => authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email already exists.");
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithNullRequest_ShouldThrow()
    {
        // Arrange
        var authService = CreateAuthService();

        // Act & Assert
        await authService.Invoking(x => x.RegisterAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Email = "test@example.com",
            Name = "Test User",
            Role = "User",
            IsActive = true
        };

        var userDto = new UserDto 
        { 
            Id = user.Id, 
            Username = user.Username, 
            Email = user.Email,
            Name = user.Name,
            Role = user.Role
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockTokenService.Setup(x => x.GenerateAccessToken(user.Id, user.Username, user.Role))
            .Returns("access_token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");
        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);
        _mockUserRepository.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var authService = CreateAuthService();

        // Act
        var result = await authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ShouldThrow()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "Password123!"
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        var authService = CreateAuthService();

        // Act
        Func<Task> act = () => authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ShouldThrow()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Email = "test@example.com",
            Name = "Test User",
            Role = "User",
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var authService = CreateAuthService();

        // Act
        Func<Task> act = () => authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ShouldThrow()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "inactiveuser",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "inactiveuser",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Email = "inactive@example.com",
            Name = "Inactive User",
            Role = "User",
            IsActive = false
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var authService = CreateAuthService();

        // Act
        Func<Task> act = () => authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User account is inactive.");
    }

    #endregion

    #region GetUserAsync Tests

    [Fact]
    public async Task GetUserAsync_WithValidUserId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Username = "testuser",
            Email = "test@example.com",
            Name = "Test User",
            Role = "User",
            IsActive = true
        };

        var userDto = new UserDto 
        { 
            Id = user.Id, 
            Username = user.Username, 
            Email = user.Email,
            Name = user.Name,
            Role = user.Role
        };

        _mockCacheService.Setup(x => x.GetAsync<UserDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto)null!);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockMapper.Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);
        _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<UserDto>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        var authService = CreateAuthService();

        // Act
        var result = await authService.GetUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetUserAsync_UserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockCacheService.Setup(x => x.GetAsync<UserDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto)null!);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        var authService = CreateAuthService();

        // Act
        var result = await authService.GetUserAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserAsync_WithCachedUser_ShouldReturnCachedResultWithoutDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cachedUserDto = new UserDto 
        { 
            Id = userId, 
            Username = "cacheduser",
            Email = "cached@example.com",
            Name = "Cached User",
            Role = "User"
        };

        _mockCacheService.Setup(x => x.GetAsync<UserDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedUserDto);

        var authService = CreateAuthService();

        // Act
        var result = await authService.GetUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("cacheduser");
        _mockUserRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
