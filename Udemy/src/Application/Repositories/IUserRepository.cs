namespace Udemy.Application.Repositories;

using System.Linq.Expressions;
using Udemy.Domain.Models;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users.
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds users by predicate.
    /// </summary>
    Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user exists by predicate.
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new user.
    /// </summary>
    Task<User> AddAsync(User entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user.
    /// </summary>
    Task<User> UpdateAsync(User entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    Task DeleteAsync(User entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a refresh token by token string.
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a refresh token.
    /// </summary>
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}
