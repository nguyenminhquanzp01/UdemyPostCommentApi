namespace Udemy.Domain.Models;

/// <summary>
/// Represents a blog post in the system.
/// </summary>
public class Post
{
    /// <summary>
    /// Gets or sets the unique identifier for the post.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the title of the post.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the content of the post.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the post author.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the post was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the post was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Navigation property for the user who created this post.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Navigation property for comments on this post.
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
