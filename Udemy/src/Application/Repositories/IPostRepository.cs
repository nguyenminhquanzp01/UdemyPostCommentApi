namespace Udemy.Application.Repositories;

using System.Linq.Expressions;
using Udemy.Domain.Models;

/// <summary>
/// Repository interface for Post entity operations.
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Gets a post by ID.
    /// </summary>
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all posts.
    /// </summary>
    Task<IEnumerable<Post>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets posts by user ID with pagination.
    /// </summary>
    Task<(IList<Post> Posts, int TotalCount)> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all posts with pagination.
    /// </summary>
    Task<(IList<Post> Posts, int TotalCount)> GetAllWithPaginationAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a post by ID with comments.
    /// </summary>
    Task<Post?> GetWithCommentsAsync(Guid postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds posts by predicate.
    /// </summary>
    Task<IEnumerable<Post>> FindAsync(Expression<Func<Post, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if post exists by predicate.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<Post, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new post.
    /// </summary>
    Task<Post> AddAsync(Post entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a post.
    /// </summary>
    Task<Post> UpdateAsync(Post entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a post.
    /// </summary>
    Task DeleteAsync(Post entity, CancellationToken cancellationToken = default);
}
