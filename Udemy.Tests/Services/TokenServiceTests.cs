using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Moq;
using Xunit;
using FluentAssertions;
using Udemy.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Udemy.Tests.Services;

/// <summary>
/// Unit tests for TokenService
/// </summary>
public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<TokenService>> _mockLogger;

    public TokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<TokenService>>();
    }

    private TokenService CreateTokenService()
    {
        return new TokenService(_mockConfiguration.Object, _mockLogger.Object);
    }

    private void SetupJwtConfiguration(
        string secretKey = "this-is-a-very-long-secret-key-for-testing-purposes-minimum-256-bits",
        string issuer = "UdemyApiIssuer",
        string audience = "UdemyApiAudience",
        string expirationMinutes = "15")
    {
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x[It.Is<string>(s => s == "SecretKey")]).Returns(secretKey);
        jwtSection.Setup(x => x[It.Is<string>(s => s == "Issuer")]).Returns(issuer);
        jwtSection.Setup(x => x[It.Is<string>(s => s == "Audience")]).Returns(audience);
        jwtSection.Setup(x => x[It.Is<string>(s => s == "ExpirationMinutes")]).Returns(expirationMinutes);

        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);
    }

    #region GenerateAccessToken Tests

    [Fact]
    public void GenerateAccessToken_WithValidInput_ShouldReturnValidToken()
    {
        // Arrange
        SetupJwtConfiguration();
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var service = CreateTokenService();

        // Act
        var token = service.GenerateAccessToken(userId, username, role);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_WithValidInput_TokenShouldContainCorrectClaims()
    {
        // Arrange
        SetupJwtConfiguration();
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "Admin";

        var service = CreateTokenService();

        // Act
        var token = service.GenerateAccessToken(userId, username, role);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);
    }

    [Fact]
    public void GenerateAccessToken_WithValidInput_TokenShouldContainCorrelationId()
    {
        // Arrange
        SetupJwtConfiguration();
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var service = CreateTokenService();

        // Act
        var token = service.GenerateAccessToken(userId, username, role);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == "CorrelationId");
        var correlationIdClaim = jwtToken.Claims.First(c => c.Type == "CorrelationId");
        Guid.TryParse(correlationIdClaim.Value, out _).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_WithValidInput_TokenExpirationShouldBeSet()
    {
        // Arrange
        SetupJwtConfiguration(expirationMinutes: "30");
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var service = CreateTokenService();
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = service.GenerateAccessToken(userId, username, role);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeAfter(beforeGeneration.AddMinutes(29));
        jwtToken.ValidTo.Should().BeBefore(beforeGeneration.AddMinutes(31));
    }

    [Fact]
    public void GenerateAccessToken_WithNullUsername_ShouldThrowArgumentNullException()
    {
        // Arrange
        SetupJwtConfiguration();
        var userId = Guid.NewGuid();
        var role = "User";

        var service = CreateTokenService();

        // Act
        Action act = () => service.GenerateAccessToken(userId, null!, role);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("username");
    }

    [Fact]
    public void GenerateAccessToken_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Arrange
        SetupJwtConfiguration();
        var userId = Guid.NewGuid();
        var username = "testuser";

        var service = CreateTokenService();

        // Act
        Action act = () => service.GenerateAccessToken(userId, username, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("role");
    }

    [Fact]
    public void GenerateAccessToken_WithMissingJwtSecretKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x[It.Is<string>(s => s == "SecretKey")]).Returns((string)null!);
        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var service = CreateTokenService();

        // Act
        Action act = () => service.GenerateAccessToken(userId, username, role);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT SecretKey not configured");
    }

    [Fact]
    public void GenerateAccessToken_WithDifferentRoles_ShouldGenerateTokensWithCorrectRoles()
    {
        // Arrange
        SetupJwtConfiguration();
        var userId = Guid.NewGuid();
        var username = "testuser";
        var roles = new[] { "User", "Admin", "Moderator" };

        var service = CreateTokenService();
        var handler = new JwtSecurityTokenHandler();

        // Act & Assert
        foreach (var role in roles)
        {
            var token = service.GenerateAccessToken(userId, username, role);
            var jwtToken = handler.ReadJwtToken(token);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);
        }
    }

    [Fact]
    public void GenerateAccessToken_WithMultipleCalls_ShouldGenerateDifferentCorrelationIds()
    {
        // Arrange
        SetupJwtConfiguration();
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var service = CreateTokenService();
        var handler = new JwtSecurityTokenHandler();

        // Act
        var token1 = service.GenerateAccessToken(userId, username, role);
        var token2 = service.GenerateAccessToken(userId, username, role);

        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var correlationId1 = jwtToken1.Claims.First(c => c.Type == "CorrelationId").Value;
        var correlationId2 = jwtToken2.Claims.First(c => c.Type == "CorrelationId").Value;

        // Assert
        correlationId1.Should().NotBe(correlationId2);
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Arrange
        SetupJwtConfiguration();
        var service = CreateTokenService();

        // Act
        var refreshToken = service.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Arrange
        SetupJwtConfiguration();
        var service = CreateTokenService();

        // Act
        var refreshToken = service.GenerateRefreshToken();

        // Assert
        Action act = () => Convert.FromBase64String(refreshToken);
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateDifferentTokensOnEachCall()
    {
        // Arrange
        SetupJwtConfiguration();
        var service = CreateTokenService();

        // Act
        var token1 = service.GenerateRefreshToken();
        var token2 = service.GenerateRefreshToken();
        var token3 = service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateLongEnoughToken()
    {
        // Arrange
        SetupJwtConfiguration();
        var service = CreateTokenService();

        // Act
        var refreshToken = service.GenerateRefreshToken();
        var decodedBytes = Convert.FromBase64String(refreshToken);

        // Assert - Should be 64 bytes (512 bits)
        decodedBytes.Should().HaveCount(64);
    }

    #endregion

    #region GetPrincipalFromExpiredToken Tests

    [Fact]
    public void GetPrincipalFromExpiredToken_WithValidExpiredToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        SetupJwtConfiguration(expirationMinutes: "-10"); // Expired 10 minutes ago
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var tokenService = CreateTokenService();
        var expiredToken = tokenService.GenerateAccessToken(userId, username, role);

        // Act
        var principal = tokenService.GetPrincipalFromExpiredToken(expiredToken);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithNullToken_ShouldThrowArgumentNullException()
    {
        // Arrange
        SetupJwtConfiguration();
        var service = CreateTokenService();

        // Act
        Action act = () => service.GetPrincipalFromExpiredToken(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("expiredToken");
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        SetupJwtConfiguration();
        var service = CreateTokenService();
        var invalidToken = "invalid.token.here";

        // Act
        var principal = service.GetPrincipalFromExpiredToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithMissingSecretKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x[It.Is<string>(s => s == "SecretKey")]).Returns((string)null!);
        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

        var service = CreateTokenService();

        // Act
        Action act = () => service.GetPrincipalFromExpiredToken("any.token");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT SecretKey not configured");
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithTokenGeneratedWithDifferentSecret_ShouldReturnNull()
    {
        // Arrange
        SetupJwtConfiguration(secretKey: "original-secret-key-for-testing-purposes-minimum-256-bits");
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var tokenService = CreateTokenService();
        var token = tokenService.GenerateAccessToken(userId, username, role);

        // Change the secret key for validation
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.Setup(x => x[It.Is<string>(s => s == "SecretKey")]).Returns("different-secret-key-for-testing-purposes-minimum-256");
        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);

        var newService = CreateTokenService();

        // Act
        var principal = newService.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithValidToken_ShouldContainAllClaims()
    {
        // Arrange
        SetupJwtConfiguration(expirationMinutes: "-5");
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "Admin";

        var service = CreateTokenService();
        var token = service.GenerateAccessToken(userId, username, role);

        // Act
        var principal = service.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);
        principal.Claims.Should().Contain(c => c.Type == "CorrelationId");
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldNotValidateTokenExpiration()
    {
        // Arrange - Create a token that expired long ago
        SetupJwtConfiguration(expirationMinutes: "-1440"); // Expired 24 hours ago
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "User";

        var service = CreateTokenService();
        var expiredToken = service.GenerateAccessToken(userId, username, role);

        // Act
        var principal = service.GetPrincipalFromExpiredToken(expiredToken);

        // Assert - Should still return principal even though token is very old
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithEmptyString_ShouldReturnNull()
    {
        // Arrange
        SetupJwtConfiguration();
        var service = CreateTokenService();

        // Act
        var principal = service.GetPrincipalFromExpiredToken("");

        // Assert
        principal.Should().BeNull();
    }

    #endregion
}
