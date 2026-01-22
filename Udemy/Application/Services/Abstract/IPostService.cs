namespace Udemy.Application.Services;

using Udemy.Application.DTOs;
using Udemy.Domain.Models;

/// <summary>
/// Interface for post operations.
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Creates a new post.
    /// </summary>
    Task<PostDto> CreatePostAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a post by ID.
    /// </summary>
    Task<PostDetailsDto?> GetPostAsync(Guid postId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all posts with pagination.
    /// </summary>
    Task<(IList<PostDto> Posts, int TotalCount)> GetPostsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets posts by a specific user.
    /// </summary>
    Task<(IList<PostDto> Posts, int TotalCount)> GetUserPostsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a post.
    /// </summary>
    Task<PostDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a post.
    /// </summary>
    Task DeletePostAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default);
}
