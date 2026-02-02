namespace Udemy.Domain.Models;

/// <summary>
/// Represents a refresh token for JWT authentication.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Gets or sets the unique identifier for the refresh token.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the token value.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Gets or sets the user ID this token belongs to.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the token was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the token expires.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the token was revoked (if applicable).
    /// Null if the token has not been revoked.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Gets a value indicating whether the token is still valid.
    /// </summary>
    public bool IsValid => RevokedAt == null && ExpiresAt > DateTimeOffset.UtcNow;

    /// <summary>
    /// Navigation property for the user this token belongs to.
    /// </summary>
    public virtual User? User { get; set; }
}
