namespace Udemy.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Udemy.Application.DTOs;
using Udemy.Application.Services;

/// <summary>
/// API controller for comment operations.
/// </summary>
[ApiController]
[Route("api/comments")]
[EnableRateLimiting("per-user")]
public class CommentsController(
    ICommentService commentService,
    ILogger<CommentsController> logger) : ControllerBase
{
    private readonly ICommentService _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
    private readonly ILogger<CommentsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets a specific comment.
    /// </summary>
    /// <param name="commentId">The comment ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The comment details.</returns>
    [HttpGet("{commentId}")]
    [AllowAnonymous]
    public async Task<ActionResult<CommentDto>> GetComment(
        Guid commentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving comment {CommentId}", commentId);

        var comment = await _commentService.GetCommentAsync(commentId, cancellationToken).ConfigureAwait(false);
        if (comment == null)
        {
            return NotFound();
        }

        return Ok(comment);
    }

    /// <summary>
    /// Updates a comment.
    /// </summary>
    /// <param name="commentId">The comment ID.</param>
    /// <param name="request">The update comment request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated comment.</returns>
    [HttpPut("{commentId}")]
    [EnableRateLimiting("per-user")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> UpdateComment(
        Guid commentId,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Updating comment {CommentId} by user {UserId}", commentId, parsedUserId);

        try
        {
            var comment = await _commentService.UpdateCommentAsync(commentId, parsedUserId, request, cancellationToken).ConfigureAwait(false);
            return Ok(comment);
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
    /// Deletes a comment.
    /// </summary>
    /// <param name="commentId">The comment ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{commentId}")]
    [EnableRateLimiting("per-user")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("Deleting comment {CommentId} by user {UserId}", commentId, parsedUserId);

        try
        {
            await _commentService.DeleteCommentAsync(commentId, parsedUserId, cancellationToken).ConfigureAwait(false);
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
    /// Gets comment replies.
    /// </summary>
    /// <param name="commentId">The comment ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of replies to the comment.</returns>
    [HttpGet("{commentId}/replies")]
    [AllowAnonymous]
    public async Task<ActionResult<IList<CommentDto>>> GetCommentReplies(
        Guid commentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving replies for comment {CommentId}", commentId);

        var replies = await _commentService.GetCommentRepliesAsync(commentId, cancellationToken).ConfigureAwait(false);
        return Ok(replies);
    }
}
