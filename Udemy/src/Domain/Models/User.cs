namespace Udemy.Domain.Models;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique username.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Gets or sets the hashed password.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's role for authorization.
    /// </summary>
    public string Role { get; set; } = "User";

    /// <summary>
    /// Gets or sets the timestamp when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the user was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for posts created by this user.
    /// </summary>
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    /// <summary>
    /// Navigation property for comments created by this user.
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Navigation property for refresh tokens associated with this user.
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
