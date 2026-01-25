namespace Udemy.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Udemy.Application.DTOs;
using Udemy.Application.Services;

/// <summary>
/// API controller for post operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PostsController(
    IPostService postService,
    ICommentService commentService,
    ILogger<PostsController> logger) : ControllerBase
{
    private readonly IPostService _postService = postService ?? throw new ArgumentNullException(nameof(postService));
    private readonly ICommentService _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
    private readonly ILogger<PostsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets all posts with pagination and optional search.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The page size (default: 10).</param>
    /// <param name="search">Optional search term for filtering posts.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of posts.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving posts (Page: {Page}, PageSize: {PageSize}, Search: {Search})", page, pageSize, search);

        var (posts, totalCount) = await _postService.GetPostsAsync(page, pageSize, cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            data = posts,
            pagination = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Gets a specific post by ID with comments.
    /// </summary>
    /// <param name="id">The post ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The post details with comments.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostDetailsDto>> GetPost(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving post {PostId}", id);

        var post = await _postService.GetPostAsync(id, cancellationToken).ConfigureAwait(false);
        if (post == null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="request">The create post request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created post.</returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> CreatePost(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Creating post for user {UserId}", parsedUserId);

        var post = await _postService.CreatePostAsync(parsedUserId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    /// <summary>
    /// Updates a post.
    /// </summary>
    /// <param name="id">The post ID.</param>
    /// <param name="request">The update post request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated post.</returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PostDto>> UpdatePost(
        Guid id,
        [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Updating post {PostId} by user {UserId}", id, parsedUserId);

        try
        {
            var post = await _postService.UpdatePostAsync(id, parsedUserId, request, cancellationToken).ConfigureAwait(false);
            return Ok(post);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a post.
    /// </summary>
    /// <param name="id">The post ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Deleting post {PostId} by user {UserId}", id, parsedUserId);

        try
        {
            await _postService.DeletePostAsync(id, parsedUserId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new comment on a post.
    /// </summary>
    /// <param name="postId">The post ID.</param>
    /// <param name="request">The create comment request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created comment.</returns>
    [HttpPost("{postId}/comments")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> CreateComment(
        Guid postId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Creating comment on post {PostId} by user {UserId}", postId, parsedUserId);

        try
        {
            var comment = await _commentService.CreateCommentAsync(postId, parsedUserId, request, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction("GetComment", "Comments", new { commentId = comment.Id }, comment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
