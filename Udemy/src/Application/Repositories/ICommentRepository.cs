namespace Udemy.Application.Repositories;

using System.Linq.Expressions;
using Udemy.Domain.Models;

/// <summary>
/// Repository interface for Comment entity operations.
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// Gets a comment by ID.
    /// </summary>
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all comments.
    /// </summary>
    Task<IEnumerable<Comment>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comments by post ID.
    /// </summary>
    Task<IList<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets replies to a comment.
    /// </summary>
    Task<IList<Comment>> GetRepliesAsync(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comment by ID with user and replies.
    /// </summary>
    Task<Comment?> GetWithDetailsAsync(Guid commentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds comments by predicate.
    /// </summary>
    Task<IEnumerable<Comment>> FindAsync(Expression<Func<Comment, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if comment exists by predicate.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<Comment, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new comment.
    /// </summary>
    Task<Comment> AddAsync(Comment entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a comment.
    /// </summary>
    Task<Comment> UpdateAsync(Comment entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a comment.
    /// </summary>
    Task DeleteAsync(Comment entity, CancellationToken cancellationToken = default);
}
