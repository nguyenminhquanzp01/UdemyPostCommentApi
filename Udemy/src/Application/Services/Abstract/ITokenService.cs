namespace Udemy.Application.Services;

/// <summary>
/// Interface for JWT token operations.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for the specified user ID and role.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="username">The username.</param>
    /// <param name="role">The user role.</param>
    /// <returns>The JWT access token.</returns>
    string GenerateAccessToken(Guid userId, string username, string role);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>A new refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Gets the principal from an expired access token.
    /// </summary>
    /// <param name="expiredToken">The expired access token.</param>
    /// <returns>The claims principal or null if validation fails.</returns>
    System.Security.Claims.ClaimsPrincipal? GetPrincipalFromExpiredToken(string expiredToken);
}
