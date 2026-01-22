namespace Udemy.Application.DTOs;

/// <summary>
/// Author information DTO (ID and Name only).
/// </summary>
public class AuthorDto
{
    /// <summary>
    /// Gets or sets the author user ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Create post request DTO.
/// </summary>
public class CreatePostRequest
{
    /// <summary>
    /// Gets or sets the post title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the post content.
    /// </summary>
    public required string Content { get; set; }
}

/// <summary>
/// Update post request DTO.
/// </summary>
public class UpdatePostRequest
{
    /// <summary>
    /// Gets or sets the post title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the post content.
    /// </summary>
    public required string Content { get; set; }
}

/// <summary>
/// Post response DTO.
/// </summary>
public class PostDto
{
    /// <summary>
    /// Gets or sets the post ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the post title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the post content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author information.
    /// </summary>
    public AuthorDto? Author { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the comment count.
    /// </summary>
    public int CommentCount { get; set; }
}

/// <summary>
/// Post details DTO with comments.
/// </summary>
public class PostDetailsDto : PostDto
{
    /// <summary>
    /// Gets or sets the list of top-level comments.
    /// </summary>
    public IList<CommentDto> Comments { get; set; } = new List<CommentDto>();
}
