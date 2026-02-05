namespace Udemy.Infrastructure.Repositories;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Udemy.Application.Repositories;
using Udemy.Domain.Data;
using Udemy.Domain.Models;

/// <summary>
/// Repository implementation for Comment entity operations.
/// </summary>
public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CommentRepository> _logger;

    public CommentRepository(AppDbContext dbContext, ILogger<CommentRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Comment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Comment>> FindAsync(Expression<Func<Comment, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<Comment?> FindOneAsync(Expression<Func<Comment, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Comment> AddAsync(Comment entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbContext.Comments.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Comment added: {CommentId}", entity.Id);
        return entity;
    }

    public async Task<Comment> UpdateAsync(Comment entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbContext.Comments.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Comment updated: {CommentId}", entity.Id);
        return entity;
    }

    public async Task DeleteAsync(Comment entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbContext.Comments.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Comment deleted: {CommentId}", entity.Id);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Comment, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<Comment, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments.CountAsync(predicate, cancellationToken);
    }

    public async Task<IList<Comment>> GetByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var comments = await _dbContext.Comments
            .Where(c => c.PostId == postId && c.ParentId == null)
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {CommentCount} top-level comments for post {PostId}", comments.Count, postId);
        return comments;
    }

    public async Task<IList<Comment>> GetRepliesAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var replies = await _dbContext.Comments
            .Where(c => c.ParentId == commentId)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {ReplyCount} replies for comment {CommentId}", replies.Count, commentId);
        return replies;
    }

    public async Task<Comment?> GetWithDetailsAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _dbContext.Comments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment != null)
        {
            _logger.LogInformation("Comment {CommentId} retrieved with details", commentId);
        }

        return comment;
    }
}
