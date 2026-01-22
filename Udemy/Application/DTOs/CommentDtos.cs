namespace Udemy.Application.DTOs;

/// <summary>
/// Create comment request DTO.
/// </summary>
public class CreateCommentRequest
{
    /// <summary>
    /// Gets or sets the comment content.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the parent comment ID for nested replies (optional).
    /// </summary>
    public Guid? ParentId { get; set; }
}

/// <summary>
/// Update comment request DTO.
/// </summary>
public class UpdateCommentRequest
{
    /// <summary>
    /// Gets or sets the updated comment content.
    /// </summary>
    public required string Content { get; set; }
}

/// <summary>
/// Comment response DTO.
/// </summary>
public class CommentDto
{
    /// <summary>
    /// Gets or sets the comment ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the comment content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author information.
    /// </summary>
    public AuthorDto? Author { get; set; }

    /// <summary>
    /// Gets or sets the post ID.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Gets or sets the parent comment ID if this is a reply.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the depth level (0 for root, 1-3 for nested).
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of replies to this comment.
    /// </summary>
    public IList<CommentDto> Replies { get; set; } = new List<CommentDto>();
}

/// <summary>
/// Comment tree DTO for hierarchical comment structure.
/// </summary>
public class CommentTreeDto
{
    /// <summary>
    /// Gets or sets the comment ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the comment content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author information.
    /// </summary>
    public AuthorDto? Author { get; set; }

    /// <summary>
    /// Gets or sets the parent comment ID if this is a reply.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of child replies in hierarchical structure.
    /// </summary>
    public IList<CommentTreeDto> Replies { get; set; } = new List<CommentTreeDto>();
}
