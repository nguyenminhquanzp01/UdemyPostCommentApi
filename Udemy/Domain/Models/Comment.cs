namespace Udemy.Domain.Models;

/// <summary>
/// Represents a comment on a post with support for nested replies up to 3 levels deep.
/// </summary>
public class Comment
{
    /// <summary>
    /// Gets or sets the unique identifier for the comment.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the content of the comment.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the comment author.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the post ID that this comment is on.
    /// </summary>
    public required Guid PostId { get; set; }

    /// <summary>
    /// Gets or sets the parent comment ID for nested comments (max 3 levels deep).
    /// Null if this is a top-level comment.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the depth level of the comment (0 for root, 1-3 for nested).
    /// </summary>
    public int Depth { get; set; } = 0;

    /// <summary>
    /// Gets or sets the timestamp when the comment was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the comment was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Navigation property for the user who created this comment.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Navigation property for the post this comment is on.
    /// </summary>
    public virtual Post? Post { get; set; }

    /// <summary>
    /// Navigation property for the parent comment if this is a reply.
    /// </summary>
    public virtual Comment? Parent { get; set; }

    /// <summary>
    /// Navigation property for child comments (replies to this comment).
    /// </summary>
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
