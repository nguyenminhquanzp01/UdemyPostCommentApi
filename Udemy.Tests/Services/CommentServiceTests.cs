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
/// Unit tests for CommentService using Repository pattern
/// </summary>
public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _mockCommentRepository;
    private readonly Mock<IPostRepository> _mockPostRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CommentService>> _mockLogger;

    public CommentServiceTests()
    {
        _mockCommentRepository = new Mock<ICommentRepository>();
        _mockPostRepository = new Mock<IPostRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CommentService>>();
    }

    private CommentService CreateCommentService()
    {
        return new CommentService(
            _mockCommentRepository.Object,
            _mockPostRepository.Object,
            _mockCacheService.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    #region CreateCommentAsync Tests

    [Fact]
    public async Task CreateCommentAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            Content = "This is a test comment",
            ParentId = null
        };

        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test Content",
            UserId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var newComment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            PostId = postId,
            UserId = userId,
            Depth = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var commentDto = new CommentDto
        {
            Id = newComment.Id,
            Content = newComment.Content,
            PostId = postId,
            CreatedAt = newComment.CreatedAt,
            UpdatedAt = newComment.UpdatedAt
        };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _mockCommentRepository.Setup(x => x.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newComment);
        _mockMapper.Setup(x => x.Map<CommentDto>(It.IsAny<Comment>()))
            .Returns(commentDto);
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateCommentService();

        // Act
        var result = await service.CreateCommentAsync(postId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(request.Content);
        _mockCommentRepository.Verify(x => x.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCommentAsync_PostNotFound_ShouldThrow()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest { Content = "Test comment" };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post)null!);

        var service = CreateCommentService();

        // Act
        Func<Task> act = () => service.CreateCommentAsync(postId, userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Post not found.");
    }

    [Fact]
    public async Task CreateCommentAsync_WithReplyToExistingComment_ShouldSucceed()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();

        var request = new CreateCommentRequest
        {
            Content = "This is a reply",
            ParentId = parentCommentId
        };

        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test Content",
            UserId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var parentComment = new Comment
        {
            Id = parentCommentId,
            PostId = postId,
            Content = "Parent comment",
            UserId = Guid.NewGuid(),
            Depth = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var newReply = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            PostId = postId,
            UserId = userId,
            ParentId = parentCommentId,
            Depth = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var replyDto = new CommentDto
        {
            Id = newReply.Id,
            Content = newReply.Content,
            PostId = postId,
            ParentId = parentCommentId,
            CreatedAt = newReply.CreatedAt,
            UpdatedAt = newReply.UpdatedAt
        };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _mockCommentRepository.Setup(x => x.GetByIdAsync(parentCommentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(parentComment);
        _mockCommentRepository.Setup(x => x.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newReply);
        _mockMapper.Setup(x => x.Map<CommentDto>(It.IsAny<Comment>()))
            .Returns(replyDto);
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateCommentService();

        // Act
        var result = await service.CreateCommentAsync(postId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("This is a reply");
    }

    [Fact]
    public async Task CreateCommentAsync_ParentCommentNotFound_ShouldThrow()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();

        var request = new CreateCommentRequest
        {
            Content = "This is a reply",
            ParentId = parentCommentId
        };

        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test Content",
            UserId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockPostRepository.Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);
        _mockCommentRepository.Setup(x => x.GetByIdAsync(parentCommentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment)null!);

        var service = CreateCommentService();

        // Act
        Func<Task> act = () => service.CreateCommentAsync(postId, userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Parent comment not found on this post.");
    }

    #endregion

    #region GetCommentAsync Tests

    [Fact]
    public async Task GetCommentAsync_WithValidCommentId_ShouldReturnComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var author = new User
        {
            Id = Guid.NewGuid(),
            Name = "Commenter",
            Username = "commenter",
            Email = "commenter@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User",
            IsActive = true
        };

        var comment = new Comment
        {
            Id = commentId,
            Content = "Test comment",
            User = author,
            Replies = new List<Comment>(),
            UserId = author.Id,
            PostId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var commentDto = new CommentDto
        {
            Id = commentId,
            Content = "Test comment",
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };

        _mockCommentRepository.Setup(x => x.GetWithDetailsAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        _mockMapper.Setup(x => x.Map<CommentDto>(It.IsAny<Comment>()))
            .Returns(commentDto);

        var service = CreateCommentService();

        // Act
        var result = await service.GetCommentAsync(commentId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(commentId);
        result.Content.Should().Be("Test comment");
    }

    [Fact]
    public async Task GetCommentAsync_CommentNotFound_ShouldReturnNull()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        _mockCommentRepository.Setup(x => x.GetWithDetailsAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment)null!);

        var service = CreateCommentService();

        // Act
        var result = await service.GetCommentAsync(commentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPostCommentsAsync Tests

    [Fact]
    public async Task GetPostCommentsAsync_WithValidPostId_ShouldReturnComments()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var author1 = new User
        {
            Id = Guid.NewGuid(),
            Name = "User 1",
            Username = "user1",
            Email = "user1@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User",
            IsActive = true
        };
        var author2 = new User
        {
            Id = Guid.NewGuid(),
            Name = "User 2",
            Username = "user2",
            Email = "user2@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = "User",
            IsActive = true
        };

        var comments = new List<Comment>
        {
            new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Comment 1",
                PostId = postId,
                User = author1,
                Replies = new List<Comment>(),
                CreatedAt = DateTimeOffset.UtcNow,
                UserId = author1.Id
            },
            new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Comment 2",
                PostId = postId,
                User = author2,
                Replies = new List<Comment>(),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                UserId = author2.Id
            }
        };

        var commentDtos = comments.Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        _mockCommentRepository.Setup(x => x.GetByPostIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);
        _mockMapper.Setup(x => x.Map<IList<CommentDto>>(It.IsAny<ICollection<Comment>>()))
            .Returns(commentDtos);

        var service = CreateCommentService();

        // Act
        var result = await service.GetPostCommentsAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPostCommentsAsync_WithEmptyComments_ShouldReturnEmptyList()
    {
        // Arrange
        var postId = Guid.NewGuid();

        _mockCommentRepository.Setup(x => x.GetByPostIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());
        _mockMapper.Setup(x => x.Map<IList<CommentDto>>(It.IsAny<ICollection<Comment>>()))
            .Returns(new List<CommentDto>());

        var service = CreateCommentService();

        // Act
        var result = await service.GetPostCommentsAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetCommentRepliesAsync Tests

    [Fact]
    public async Task GetCommentRepliesAsync_WithValidCommentId_ShouldReturnReplies()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var replies = new List<Comment>
        {
            new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Reply 1",
                ParentId = commentId,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new Comment
            {
                Id = Guid.NewGuid(),
                Content = "Reply 2",
                ParentId = commentId,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };

        var replyDtos = replies.Select(r => new CommentDto
        {
            Id = r.Id,
            Content = r.Content,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        _mockCommentRepository.Setup(x => x.GetRepliesAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(replies);
        _mockMapper.Setup(x => x.Map<IList<CommentDto>>(It.IsAny<ICollection<Comment>>()))
            .Returns(replyDtos);

        var service = CreateCommentService();

        // Act
        var result = await service.GetCommentRepliesAsync(commentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion

    #region UpdateCommentAsync Tests

    [Fact]
    public async Task UpdateCommentAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Content = "Updated content" };

        var comment = new Comment
        {
            Id = commentId,
            Content = "Original content",
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var updatedComment = new Comment
        {
            Id = commentId,
            Content = "Updated content",
            PostId = postId,
            UserId = userId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var commentDto = new CommentDto
        {
            Id = commentId,
            Content = "Updated content",
            CreatedAt = updatedComment.CreatedAt,
            UpdatedAt = updatedComment.UpdatedAt
        };

        _mockCommentRepository.Setup(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        _mockCommentRepository.Setup(x => x.UpdateAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedComment);
        _mockMapper.Setup(x => x.Map<CommentDto>(It.IsAny<Comment>()))
            .Returns(commentDto);
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateCommentService();

        // Act
        var result = await service.UpdateCommentAsync(commentId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be("Updated content");
    }

    [Fact]
    public async Task UpdateCommentAsync_CommentNotFound_ShouldThrow()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Content = "Updated content" };

        _mockCommentRepository.Setup(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment)null!);

        var service = CreateCommentService();

        // Act
        Func<Task> act = () => service.UpdateCommentAsync(commentId, userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Comment not found.");
    }

    [Fact]
    public async Task UpdateCommentAsync_Unauthorized_ShouldThrow()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Content = "Updated content" };

        var comment = new Comment
        {
            Id = commentId,
            UserId = differentUserId,
            Content = "Original content",
            PostId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockCommentRepository.Setup(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        var service = CreateCommentService();

        // Act
        Func<Task> act = () => service.UpdateCommentAsync(commentId, userId, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You are not authorized to update this comment.");
    }

    #endregion

    #region DeleteCommentAsync Tests

    [Fact]
    public async Task DeleteCommentAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            PostId = postId,
            UserId = userId,
            Content = "Comment to be deleted",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockCommentRepository.Setup(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);
        _mockCommentRepository.Setup(x => x.DeleteAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateCommentService();

        // Act
        await service.DeleteCommentAsync(commentId, userId);

        // Assert
        _mockCommentRepository.Verify(x => x.DeleteAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCommentAsync_CommentNotFound_ShouldThrow()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockCommentRepository.Setup(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment)null!);

        var service = CreateCommentService();

        // Act
        Func<Task> act = () => service.DeleteCommentAsync(commentId, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Comment not found.");
    }

    [Fact]
    public async Task DeleteCommentAsync_Unauthorized_ShouldThrow()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            UserId = differentUserId,
            Content = "Original content",
            PostId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockCommentRepository.Setup(x => x.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        var service = CreateCommentService();

        // Act
        Func<Task> act = () => service.DeleteCommentAsync(commentId, userId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You are not authorized to delete this comment.");
    }

    #endregion
}
