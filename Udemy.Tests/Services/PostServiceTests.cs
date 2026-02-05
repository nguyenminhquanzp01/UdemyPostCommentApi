using Moq;
using Xunit;
using AutoMapper;
using FluentAssertions;
using Udemy.Application.DTOs;
using Udemy.Application.Services;
using Udemy.Application.Repositories;
using Udemy.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Udemy.Tests.Services;

/// <summary>
/// Unit tests for PostService using Repository pattern
/// </summary>
public class PostServiceTests
{
    private readonly Mock<IPostRepository> _mockPostRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<PostService>> _mockLogger;

    public PostServiceTests()
    {
        _mockPostRepository = new Mock<IPostRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<PostService>>();
    }

    private PostService CreatePostService()
    {
        return new PostService(
            _mockPostRepository.Object,
            _mockCacheService.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region CreatePostAsync Tests

    [Fact]
    public async Task CreatePostAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreatePostRequest
        {
            Title = "Test Post",
            Content = "This is a test post content."
        };

        var newPost = new Post
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var postDto = new PostDto
        {
            Id = newPost.Id,
            Title = newPost.Title,
            Content = newPost.Content,
            CreatedAt = newPost.CreatedAt,
            UpdatedAt = newPost.UpdatedAt
        };

        _mockPostRepository.Setup(x => x.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPost);
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMapper.Setup(x => x.Map<PostDto>(It.IsAny<Post>()))
            .Returns(postDto);

        var postService = CreatePostService();

        // Act
        var result = await postService.CreatePostAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Content.Should().Be(request.Content);
        _mockPostRepository.Verify(x => x.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePostAsync_WithNullRequest_ShouldThrow()
    {
        // Arrange
        var postService = CreatePostService();

        // Act & Assert
        await postService.Invoking(x => x.CreatePostAsync(Guid.NewGuid(), null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region GetPostAsync Tests

    [Fact]
    public async Task GetPostAsync_WithValidPostId_ShouldReturnPost()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Name = "Test User",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User",
            IsActive = true
        };

        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test content",
            UserId = userId,
            User = user,
            Comments = new List<Comment>(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var postDetailsDto = new PostDetailsDto
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test content",
            Comments = new List<CommentDto>(),
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };

        _mockCacheService.Setup(x => x.GetAsync<PostDetailsDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PostDetailsDto)null!);
        _mockPostRepository.Setup(x => x.GetWithCommentsAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _mockMapper.Setup(x => x.Map<PostDetailsDto>(It.IsAny<Post>()))
            .Returns(postDetailsDto);
        _mockMapper.Setup(x => x.Map<IList<CommentDto>>(It.IsAny<ICollection<Comment>>()))
            .Returns(new List<CommentDto>());
        _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<PostDetailsDto>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        var postService = CreatePostService();

        // Act
        var result = await postService.GetPostAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId);
        result.Title.Should().Be("Test Post");
    }

    [Fact]
    public async Task GetPostAsync_PostNotFound_ShouldReturnNull()
    {
        // Arrange
        var postId = Guid.NewGuid();

        _mockCacheService.Setup(x => x.GetAsync<PostDetailsDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PostDetailsDto)null!);
        _mockPostRepository.Setup(x => x.GetWithCommentsAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post)null!);

        var postService = CreatePostService();

        // Act
        var result = await postService.GetPostAsync(postId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPostAsync_WithCachedPost_ShouldReturnCachedResultWithoutDatabase()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var cachedPost = new PostDetailsDto
        {
            Id = postId,
            Title = "Cached Post",
            Content = "Cached content",
            Comments = new List<CommentDto>()
        };

        _mockCacheService.Setup(x => x.GetAsync<PostDetailsDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedPost);

        var postService = CreatePostService();

        // Act
        var result = await postService.GetPostAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Cached Post");
        _mockPostRepository.Verify(x => x.GetWithCommentsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetPostsAsync Tests

    [Fact]
    public async Task GetPostsAsync_WithValidPagination_ShouldReturnPosts()
    {
        // Arrange
        var author1 = new User
        {
            Id = Guid.NewGuid(),
            Name = "Author 1",
            Username = "author1",
            Email = "author1@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User",
            IsActive = true
        };

        var posts = new List<Post>
        {
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "Post 1",
                Content = "Content 1",
                UserId = author1.Id,
                User = author1,
                Comments = new List<Comment>(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
        };

        var postDtos = posts.Select(p => new PostDto
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            Author = new AuthorDto { Id = author1.Id, Name = author1.Name },
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            CommentCount = p.Comments.Count
        }).ToList();

        _mockPostRepository.Setup(x => x.GetAllWithPaginationAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((posts, 1));

        var postService = CreatePostService();

        // Act
        var (result, totalCount) = await postService.GetPostsAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPostsAsync_WithInvalidPage_ShouldThrow()
    {
        // Arrange
        var postService = CreatePostService();

        // Act & Assert
        await postService.Invoking(x => x.GetPostsAsync(0, 10))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetPostsAsync_WithInvalidPageSize_ShouldThrow()
    {
        // Arrange
        var postService = CreatePostService();

        // Act & Assert
        await postService.Invoking(x => x.GetPostsAsync(1, 0))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    #endregion

    #region GetUserPostsAsync Tests

    [Fact]
    public async Task GetUserPostsAsync_WithValidUserId_ShouldReturnUserPosts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var author = new User
        {
            Id = userId,
            Name = "Test Author",
            Username = "testauthor",
            Email = "author@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User",
            IsActive = true
        };

        var posts = new List<Post>
        {
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "User Post 1",
                Content = "Content 1",
                UserId = userId,
                User = author,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Comments = new List<Comment>()
            },
            new Post
            {
                Id = Guid.NewGuid(),
                Title = "User Post 2",
                Content = "Content 2",
                UserId = userId,
                User = author,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                Comments = new List<Comment>()
            }
        };

        _mockPostRepository.Setup(x => x.GetByUserIdAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((posts, 2));
        _mockMapper.Setup(x => x.Map<PostDto>(It.IsAny<Post>()))
            .Returns((Post p) => new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

        var postService = CreatePostService();

        // Act
        var (result, totalCount) = await postService.GetUserPostsAsync(userId, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetUserPostsAsync_WithInvalidPage_ShouldThrow()
    {
        // Arrange
        var postService = CreatePostService();

        // Act & Assert
        await postService.Invoking(x => x.GetUserPostsAsync(Guid.NewGuid(), 0, 10))
            .Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    #endregion

    #region UpdatePostAsync Tests

    [Fact]
    public async Task UpdatePostAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdatePostRequest
        {
            Title = "Updated Title",
            Content = "Updated Content"
        };

        var existingPost = new Post
        {
            Id = postId,
            Title = "Old Title",
            Content = "Old Content",
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var updatedPost = new Post
        {
            Id = postId,
            Title = request.Title,
            Content = request.Content,
            UserId = userId,
            CreatedAt = existingPost.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var postDto = new PostDto
        {
            Id = updatedPost.Id,
            Title = updatedPost.Title,
            Content = updatedPost.Content,
            CreatedAt = updatedPost.CreatedAt,
            UpdatedAt = updatedPost.UpdatedAt
        };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPost);
        _mockPostRepository.Setup(x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPost);
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockMapper.Setup(x => x.Map<PostDto>(It.IsAny<Post>()))
            .Returns(postDto);

        var postService = CreatePostService();

        // Act
        var result = await postService.UpdatePostAsync(postId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        _mockPostRepository.Verify(x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePostAsync_PostNotFound_ShouldThrow()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdatePostRequest { Title = "Updated", Content = "Updated" };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post)null!);

        var postService = CreatePostService();

        // Act
        Func<Task> act = () => postService.UpdatePostAsync(postId, userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Post not found.");
    }

    [Fact]
    public async Task UpdatePostAsync_Unauthorized_ShouldThrow()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var request = new UpdatePostRequest { Title = "Updated", Content = "Updated" };

        var post = new Post
        {
            Id = postId,
            Title = "Original",
            Content = "Original",
            UserId = otherUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        var postService = CreatePostService();

        // Act
        Func<Task> act = () => postService.UpdatePostAsync(postId, userId, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You are not authorized to update this post.");
    }

    #endregion

    #region DeletePostAsync Tests

    [Fact]
    public async Task DeletePostAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "Content",
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _mockPostRepository.Setup(x => x.DeleteAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var postService = CreatePostService();

        // Act
        await postService.DeletePostAsync(postId, userId);

        // Assert
        _mockPostRepository.Verify(x => x.DeleteAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePostAsync_PostNotFound_ShouldThrow()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post)null!);

        var postService = CreatePostService();

        // Act
        Func<Task> act = () => postService.DeletePostAsync(postId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Post not found.");
    }

    [Fact]
    public async Task DeletePostAsync_Unauthorized_ShouldThrow()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "Content",
            UserId = otherUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        var postService = CreatePostService();

        // Act
        Func<Task> act = () => postService.DeletePostAsync(postId, userId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You are not authorized to delete this post.");
    }

    #endregion
}
