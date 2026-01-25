# Udemy.Tests - Unit Test Project

This project contains comprehensive unit tests for the Udemy API using **xUnit**, **Moq**, and **FluentAssertions**.

## Project Structure

```
Udemy.Tests/
├── Services/
│   ├── AuthServiceTests.cs      # Unit tests for AuthService
│   ├── PostServiceTests.cs      # Unit tests for PostService
│   └── CommentServiceTests.cs   # Unit tests for CommentService
├── Validators/
│   ├── AuthValidatorsTests.cs   # Unit tests for Auth validators
│   └── PostValidatorsTests.cs   # Unit tests for Post validators
└── Udemy.Tests.csproj           # Test project configuration
```

## Technologies & Dependencies

- **xUnit**: Modern testing framework for .NET
- **Moq**: Mocking library for unit testing
- **FluentAssertions**: Assertion library for more readable assertions
- **FluentValidation.TestHelper**: Helper for testing FluentValidation validators

## Running Tests

### Using Visual Studio

1. Open **Test Explorer** (Test → Test Explorer)
2. Click **Run All Tests** or run specific test classes

### Using Command Line

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test -v d

# Run tests for specific project
dotnet test Udemy.Tests/Udemy.Tests.csproj

# Run specific test class
dotnet test --filter FullyQualifiedName~AuthServiceTests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Test Coverage

### AuthServiceTests

- ✅ **RegisterAsync**: Valid registration, duplicate username/email, null request
- ✅ **LoginAsync**: Valid credentials, invalid username, inactive user
- ✅ **GetUserAsync**: User exists, user not found, cached user retrieval

### PostServiceTests

- ✅ **CreatePostAsync**: Valid creation, null request validation
- ✅ **GetPostAsync**: Post exists, post not found, cache retrieval
- ✅ **GetPostsAsync**: Valid pagination, invalid pagination parameters
- ✅ **GetUserPostsAsync**: User posts retrieval

### CommentServiceTests

- ✅ **CreateCommentAsync**: Valid creation, non-existent post, null request
- ✅ **GetCommentAsync**: Comment exists, not found
- ✅ **GetPostCommentsAsync**: Valid retrieval, empty results
- ✅ **GetCommentRepliesAsync**: Reply retrieval
- ✅ **UpdateCommentAsync**: Valid update, authorization check
- ✅ **DeleteCommentAsync**: Valid deletion, not found, authorization

### AuthValidatorsTests

- ✅ **RegisterRequestValidator**: Valid input, empty fields, invalid email, weak password
- ✅ **LoginRequestValidator**: Valid input, empty fields

### PostValidatorsTests

- ✅ **CreatePostRequestValidator**: Valid input, empty fields, minimum length validation
- ✅ **UpdatePostRequestValidator**: Valid input, empty fields

## Mocking Strategy

The tests use **Moq** to mock dependencies:

- `AppDbContext`: Mocked to simulate database operations
- `ITokenService`: Mocked to generate tokens
- `ICacheService`: Mocked to simulate caching
- `IMapper`: Mocked to map DTOs
- `ILogger<T>`: Mocked for logging verification

### Example Mock Setup

```csharp
var mockDbContext = new Mock<AppDbContext>();
var mockTokenService = new Mock<ITokenService>();
var mockCacheService = new Mock<ICacheService>();
var mockMapper = new Mock<IMapper>();
var mockLogger = new Mock<ILogger<AuthService>>();

var authService = new AuthService(
    mockDbContext.Object,
    mockTokenService.Object,
    mockCacheService.Object,
    mockMapper.Object,
    mockLogger.Object
);
```

## Key Testing Patterns

### 1. Arrange-Act-Assert (AAA)

All tests follow the AAA pattern:

```csharp
// Arrange - Setup test data and mocks
var request = new RegisterRequest { /* ... */ };

// Act - Execute the method under test
var result = await _authService.RegisterAsync(request);

// Assert - Verify the results
result.Should().NotBeNull();
```

### 2. Mock Verification

```csharp
// Verify methods were called
mockDbContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

// Verify methods were not called
_mockDbContext.Verify(m => m.Users, Times.Never);
```

### 3. Exception Testing

```csharp
await act.Should().ThrowAsync<InvalidOperationException>()
    .WithMessage("Username already exists.");
```

## Best Practices

1. **One assertion concept per test**: Each test validates one specific behavior
2. **Descriptive test names**: `[MethodName]_[Condition]_[ExpectedResult]`
3. **Isolated tests**: No dependencies between tests
4. **Mock external dependencies**: Only test the unit in isolation
5. **Verify behavior, not implementation**: Test what the method does, not how it does it

## Adding New Tests

1. Create a new test file in the appropriate directory
2. Name it: `[ServiceName]Tests.cs`
3. Follow the AAA pattern
4. Use FluentAssertions for assertions
5. Mock all dependencies
6. Run tests to ensure they pass

## Troubleshooting

### Tests fail with "InvalidOperationException: Unable to resolve service"

- Ensure all mocks are properly configured
- Check that service dependencies are mocked

### DbSet mocking issues

- Use `It.IsAny<Expression<Func<T, bool>>>()` for LINQ expressions
- Setup `Include()` and other LINQ methods appropriately

### Async test failures

- Always use `async Task` for async test methods
- Use `await` when calling async methods
- Setup async methods with `.ReturnsAsync()`

## Contributing

When adding new services:

1. Create corresponding test class
2. Test all public methods
3. Cover success and failure scenarios
4. Maintain >80% code coverage
5. Update this README with new test coverage

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
