namespace Udemy.Application.Services;

using AutoMapper;
using BC = BCrypt.Net.BCrypt;
using Udemy.Application.DTOs;
using Udemy.Application.Repositories;
using Udemy.Domain.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service implementation for authentication operations.
/// </summary>
public class AuthService(
    IUserRepository userRepository,
    ITokenService tokenService,
    ICacheService cacheService,
    IMapper mapper,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<AuthService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string UserCacheKeyPrefix = "user:";

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
            .ConfigureAwait(false);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing username: {Username}", request.Username);
            throw new InvalidOperationException("Username already exists.");
        }

        var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false);

        if (existingEmail != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Name = request.Name,
            Password = BC.HashPassword(request.Password),
            Role = "User"
        };

        user = await _userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("User registered: {UserId} ({Username})", user.Id, user.Username);

        return await GenerateAuthResponseAsync(user, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Authenticates a user with credentials.
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
            .ConfigureAwait(false);

        if (user == null || !BC.Verify(request.Password, user.Password))
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
            throw new InvalidOperationException("User account is inactive.");
        }

        _logger.LogInformation("User logged in: {UserId} ({Username})", user.Id, user.Username);

        return await GenerateAuthResponseAsync(user, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var refreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken)
            .ConfigureAwait(false);

        if (refreshToken == null || !refreshToken.IsValid)
        {
            _logger.LogWarning("Invalid refresh token used");
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        var user = refreshToken.User;
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Refresh token for inactive user: {UserId}", user?.Id);
            throw new UnauthorizedAccessException("User account is inactive.");
        }

        _logger.LogInformation("Access token refreshed for user: {UserId}", user.Id);

        return await GenerateAuthResponseAsync(user, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        await _userRepository.RevokeRefreshTokenAsync(refreshToken, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{UserCacheKeyPrefix}{userId}";
        var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedUser != null)
        {
            return cachedUser;
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        if (user == null)
        {
            return null;
        }

        var userDto = _mapper.Map<UserDto>(user);
        await _cacheService.SetAsync(cacheKey, userDto, cancellationToken, TimeSpan.FromHours(1)).ConfigureAwait(false);

        return userDto;
    }

    /// <summary>
    /// Generates an authentication response with tokens.
    /// </summary>
    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, user.Role);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            User = _mapper.Map<UserDto>(user)
        };
    }
}
