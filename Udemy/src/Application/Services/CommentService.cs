namespace Udemy.Application.Services;

using AutoMapper;
using System.Text.Json;
using Udemy.Application.DTOs;
using Udemy.Application.Repositories;
using Udemy.Domain.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service implementation for comment operations.
/// </summary>
public class CommentService(
    ICommentRepository commentRepository,
    IPostRepository postRepository,
    ICacheService cacheService,
    IMapper mapper,
    ILogger<CommentService> logger) : ICommentService
{
    private readonly ICommentRepository _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
    private readonly IPostRepository _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<CommentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const int MaxCommentDepth = 3;

    /// <summary>
    /// Creates a new comment on a post.
    /// </summary>
    public async Task<CommentDto> CreateCommentAsync(Guid postId, Guid userId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verify post exists
        var postExists = await _postRepository.GetByIdAsync(postId, cancellationToken).ConfigureAwait(false);
        if (postExists == null)
        {
            throw new InvalidOperationException("Post not found.");
        }

        var comment = new Comment
        {
            Content = request.Content,
            UserId = userId,
            PostId = postId,
            ParentId = request.ParentId,
            Depth = 0
        };

        // If this is a reply, validate parent comment and calculate depth
        if (request.ParentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentId.Value, cancellationToken).ConfigureAwait(false);

            if (parentComment == null || parentComment.PostId != postId)
            {
                throw new InvalidOperationException("Parent comment not found on this post.");
            }

            if (parentComment.Depth >= MaxCommentDepth)
            {
                throw new InvalidOperationException($"Cannot reply to comments deeper than {MaxCommentDepth} levels.");
            }

            comment.Depth = parentComment.Depth + 1;
        }

        comment = await _commentRepository.AddAsync(comment, cancellationToken).ConfigureAwait(false);

        // Invalidate post cache
        await _cacheService.RemoveAsync($"post:{postId}", cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Comment {CommentId} created on post {PostId} by user {UserId}", comment.Id, postId, userId);

        return _mapper.Map<CommentDto>(comment);
    }

    /// <summary>
    /// Gets a comment by ID.
    /// </summary>
    public async Task<CommentDto?> GetCommentAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetWithDetailsAsync(commentId, cancellationToken).ConfigureAwait(false);

        if (comment == null)
        {
            return null;
        }

        return _mapper.Map<CommentDto>(comment);
    }

    /// <summary>
    /// Gets all top-level comments for a post.
    /// </summary>
    public async Task<IList<CommentDto>> GetPostCommentsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var comments = await _commentRepository.GetByPostIdAsync(postId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Retrieved {CommentCount} top-level comments for post {PostId}", comments.Count, postId);

        return _mapper.Map<IList<CommentDto>>(comments);
    }

    /// <summary>
    /// Gets replies to a comment.
    /// </summary>
    public async Task<IList<CommentDto>> GetCommentRepliesAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var replies = await _commentRepository.GetRepliesAsync(commentId, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Retrieved {ReplyCount} replies for comment {CommentId}", replies.Count, commentId);

        return _mapper.Map<IList<CommentDto>>(replies);
    }

    /// <summary>
    /// Updates a comment.
    /// </summary>
    public async Task<CommentDto> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken).ConfigureAwait(false);

        if (comment == null)
        {
            throw new InvalidOperationException("Comment not found.");
        }

        if (comment.UserId != userId)
        {
            _logger.LogWarning("Unauthorized update attempt for comment {CommentId} by user {UserId}", commentId, userId);
            throw new UnauthorizedAccessException("You are not authorized to update this comment.");
        }

        comment.Content = request.Content;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        comment = await _commentRepository.UpdateAsync(comment, cancellationToken).ConfigureAwait(false);

        // Invalidate post cache
        await _cacheService.RemoveAsync($"post:{comment.PostId}", cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Comment {CommentId} updated by user {UserId}", commentId, userId);

        return _mapper.Map<CommentDto>(comment);
    }

    /// <summary>
    /// Deletes a comment.
    /// </summary>
    public async Task DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId, cancellationToken).ConfigureAwait(false);

        if (comment == null)
        {
            throw new InvalidOperationException("Comment not found.");
        }

        if (comment.UserId != userId)
        {
            _logger.LogWarning("Unauthorized delete attempt for comment {CommentId} by user {UserId}", commentId, userId);
            throw new UnauthorizedAccessException("You are not authorized to delete this comment.");
        }

        await _commentRepository.DeleteAsync(comment, cancellationToken).ConfigureAwait(false);

        // Invalidate post cache
        await _cacheService.RemoveAsync($"post:{comment.PostId}", cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Comment {CommentId} deleted by user {UserId}", commentId, userId);
    }

    /// <summary>
    /// Gets comment tree for a post with hierarchical structure (cached).
    /// </summary>
    public async Task<IEnumerable<CommentTreeDto>> GetCommentTreeForPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"comments:tree:{postId}";

        // Try to get from cache
        var cachedTree = await _cacheService.GetAsync<List<CommentTreeDto>>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedTree != null && cachedTree.Any())
        {
            _logger.LogInformation("Comment tree for post {PostId} retrieved from cache", postId);
            return cachedTree;
        }

        // Verify post exists
        var postExists = await _postRepository.GetByIdAsync(postId, cancellationToken).ConfigureAwait(false);
        if (postExists == null)
        {
            throw new InvalidOperationException("Post not found.");
        }

        // Fetch all comments for the post with user information
        var comments = await _commentRepository.GetByPostIdAsync(postId, cancellationToken).ConfigureAwait(false);
        var commentsList = comments.ToList();

        // Build lookup keyed by ParentId (null for root comments)
        var lookup = commentsList.ToLookup(c => c.ParentId);

        // Build tree recursively with cycle detection
        List<CommentTreeDto> BuildTree(Guid? parentId, HashSet<Guid>? ancestors = null)
        {
            var currentAncestors = ancestors ?? new HashSet<Guid>();

            // Detect cycles to prevent infinite recursion
            if (parentId.HasValue && currentAncestors.Contains(parentId.Value))
            {
                _logger.LogWarning("Cycle detected in comment tree for parent {ParentId}", parentId);
                return new List<CommentTreeDto>();
            }

            var result = new List<CommentTreeDto>();

            foreach (var comment in lookup[parentId])
            {
                // Prepare ancestor set for child's recursion
                var childAncestors = new HashSet<Guid>(currentAncestors);
                if (parentId.HasValue)
                {
                    childAncestors.Add(parentId.Value);
                }

                var treeNode = new CommentTreeDto
                {
                    Id = comment.Id,
                    ParentId = comment.ParentId,
                    Content = comment.Content,
                    Author = _mapper.Map<AuthorDto>(comment.User),
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    Replies = BuildTree(comment.Id, childAncestors)
                };

                result.Add(treeNode);
            }

            return result;
        }

        var tree = BuildTree(null);

        // Cache the tree for 5 minutes
        await _cacheService.SetAsync(cacheKey, tree, cancellationToken, TimeSpan.FromMinutes(5)).ConfigureAwait(false);

        _logger.LogInformation("Comment tree for post {PostId} built and cached with {CommentCount} total comments", postId, commentsList.Count);

        return tree;
    }
}
