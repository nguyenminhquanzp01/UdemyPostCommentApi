namespace Udemy.Application.Services;

using Udemy.Application.DTOs;

/// <summary>
/// Interface for comment operations.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Creates a new comment on a post.
    /// </summary>
    Task<CommentDto> CreateCommentAsync(Guid postId, Guid userId, CreateCommentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a comment by ID.
    /// </summary>
    Task<CommentDto?> GetCommentAsync(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all top-level comments for a post.
    /// </summary>
    Task<IList<CommentDto>> GetPostCommentsAsync(Guid postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets replies to a comment.
    /// </summary>
    Task<IList<CommentDto>> GetCommentRepliesAsync(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a comment.
    /// </summary>
    Task<CommentDto> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a comment.
    /// </summary>
    Task DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comment tree for a post with hierarchical structure (cached).
    /// </summary>
    Task<IEnumerable<CommentTreeDto>> GetCommentTreeForPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
}

