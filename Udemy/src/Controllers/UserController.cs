namespace Udemy.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Udemy.Application.DTOs;
using Udemy.Application.Services;

/// <summary>
/// API controller for user profile operations.
/// </summary>
[ApiController]
[Route("api/user")]
[Authorize]
[EnableRateLimiting("per-user")]
public class UserController(
    IAuthService authService,
    IPostService postService,
    ILogger<UserController> logger) : ControllerBase
{
    private readonly IAuthService _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    private readonly IPostService _postService = postService ?? throw new ArgumentNullException(nameof(postService));
    private readonly ILogger<UserController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets a user's profile.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user profile.</returns>
    [HttpGet("{id}/profile")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> GetProfile(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving user profile {UserId}", id);

        var user = await _authService.GetUserAsync(id, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Gets a user's posts.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of user posts.</returns>
    [HttpGet("{id}/posts")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetUserPosts(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving posts for user {UserId} (Page: {Page}, PageSize: {PageSize})", id, page, pageSize);

        var (posts, totalCount) = await _postService.GetUserPostsAsync(id, page, pageSize, cancellationToken).ConfigureAwait(false);

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
}
