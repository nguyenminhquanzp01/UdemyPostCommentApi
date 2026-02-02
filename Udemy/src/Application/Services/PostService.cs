namespace Udemy.Application.Services;

using AutoMapper;
using Udemy.Application.DTOs;
using Udemy.Domain.Data;
using Udemy.Domain.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service implementation for post operations.
/// </summary>
public class PostService(
    AppDbContext dbContext,
    ICacheService cacheService,
    IMapper mapper,
    ILogger<PostService> logger) : IPostService
{
    private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<PostService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string PostCacheKeyPrefix = "post:";
    private const string PostListCacheKey = "posts:list";

    /// <summary>
    /// Creates a new post.
    /// </summary>
    public async Task<PostDto> CreatePostAsync(Guid userId, CreatePostRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            UserId = userId
        };

        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Invalidate list cache
        await _cacheService.RemoveAsync(PostListCacheKey, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Post created: {PostId} by user {UserId}", post.Id, userId);

        return _mapper.Map<PostDto>(post);
    }

    /// <summary>
    /// Gets a post by ID with comments.
    /// </summary>
    public async Task<PostDetailsDto?> GetPostAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{PostCacheKeyPrefix}{postId}";
        var cachedPost = await _cacheService.GetAsync<PostDetailsDto>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedPost != null)
        {
            _logger.LogDebug("Post {PostId} retrieved from cache", postId);
            return cachedPost;
        }

        var post = await _dbContext.Posts
            .Include(p => p.User)
            .Include(p => p.Comments.Where(c => c.ParentId == null))
            .ThenInclude(c => c.Replies)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken)
            .ConfigureAwait(false);

        if (post == null)
        {
            return null;
        }

        var postDetailsDto = _mapper.Map<PostDetailsDto>(post);
        postDetailsDto.Comments = _mapper.Map<IList<CommentDto>>(post.Comments);

        // Cache the post for 1 hour
        await _cacheService.SetAsync(cacheKey, postDetailsDto, cancellationToken, TimeSpan.FromHours(1)).ConfigureAwait(false);

        _logger.LogInformation("Post {PostId} retrieved", postId);

        return postDetailsDto;
    }

    /// <summary>
    /// Gets all posts with pagination.
    /// </summary>
    public async Task<(IList<PostDto> Posts, int TotalCount)> GetPostsAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var query = _dbContext.Posts
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                Author = new AuthorDto
                {
                    Id = p.User!.Id,
                    Name = p.User.Name,
                },
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CommentCount = p.Comments.Count
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Retrieved {PostCount} posts (Page {Page}, PageSize {PageSize}, Total {Total})",
            posts.Count, page, pageSize, totalCount);

        return (posts, totalCount);
    }

    /// <summary>
    /// Gets posts by a specific user.
    /// </summary>
    public async Task<(IList<PostDto> Posts, int TotalCount)> GetUserPostsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var query = _dbContext.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => _mapper.Map<PostDto>(p))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Retrieved {PostCount} posts for user {UserId}", posts.Count, userId);

        return (posts, totalCount);
    }

    /// <summary>
    /// Updates a post.
    /// </summary>
    public async Task<PostDto> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var post = await _dbContext.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken)
            .ConfigureAwait(false);

        if (post == null)
        {
            throw new InvalidOperationException("Post not found.");
        }

        if (post.UserId != userId)
        {
            _logger.LogWarning("Unauthorized update attempt for post {PostId} by user {UserId}", postId, userId);
            throw new UnauthorizedAccessException("You are not authorized to update this post.");
        }

        post.Title = request.Title;
        post.Content = request.Content;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.Posts.Update(post);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Invalidate cache
        await _cacheService.RemoveAsync($"{PostCacheKeyPrefix}{postId}", cancellationToken).ConfigureAwait(false);
        await _cacheService.RemoveAsync(PostListCacheKey, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Post {PostId} updated by user {UserId}", postId, userId);

        return _mapper.Map<PostDto>(post);
    }

    /// <summary>
    /// Deletes a post.
    /// </summary>
    public async Task DeletePostAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.Posts
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken)
            .ConfigureAwait(false);

        if (post == null)
        {
            throw new InvalidOperationException("Post not found.");
        }

        if (post.UserId != userId)
        {
            _logger.LogWarning("Unauthorized delete attempt for post {PostId} by user {UserId}", postId, userId);
            throw new UnauthorizedAccessException("You are not authorized to delete this post.");
        }

        _dbContext.Posts.Remove(post);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Invalidate cache
        await _cacheService.RemoveAsync($"{PostCacheKeyPrefix}{postId}", cancellationToken).ConfigureAwait(false);
        await _cacheService.RemoveAsync(PostListCacheKey, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Post {PostId} deleted by user {UserId}", postId, userId);
    }
}
