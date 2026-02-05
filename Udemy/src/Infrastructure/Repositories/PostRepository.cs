namespace Udemy.Infrastructure.Repositories;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Udemy.Application.Repositories;
using Udemy.Domain.Data;
using Udemy.Domain.Models;

/// <summary>
/// Repository implementation for Post entity operations.
/// </summary>
public class PostRepository : IPostRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PostRepository> _logger;

    public PostRepository(AppDbContext dbContext, ILogger<PostRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Post>> FindAsync(Expression<Func<Post, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Post?> FindOneAsync(Expression<Func<Post, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Post> AddAsync(Post entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbContext.Posts.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Post added: {PostId}", entity.Id);
        return entity;
    }

    public async Task<Post> UpdateAsync(Post entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbContext.Posts.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Post updated: {PostId}", entity.Id);
        return entity;
    }

    public async Task DeleteAsync(Post entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbContext.Posts.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Post deleted: {PostId}", entity.Id);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Post, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<Post, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Posts.CountAsync(predicate, cancellationToken);
    }

    public async Task<(IList<Post> Posts, int TotalCount)> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var query = _dbContext.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {PostCount} posts for user {UserId}", posts.Count, userId);
        return (posts, totalCount);
    }

    public async Task<(IList<Post> Posts, int TotalCount)> GetAllWithPaginationAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var query = _dbContext.Posts
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {PostCount} posts (Page {Page}, PageSize {PageSize})", posts.Count, page, pageSize);
        return (posts, totalCount);
    }

    public async Task<Post?> GetWithCommentsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.Posts
            .Include(p => p.User)
            .Include(p => p.Comments.Where(c => c.ParentId == null))
            .ThenInclude(c => c.Replies)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);

        if (post != null)
        {
            _logger.LogInformation("Post {PostId} retrieved with comments", postId);
        }

        return post;
    }
}
