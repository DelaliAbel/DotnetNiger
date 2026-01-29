# 🧪 Guide de Testing

Guide complet pour les tests dans DotnetNiger.

## Stack de Testing

- **xUnit** - Framework de tests
- **Moq** - Mocking framework
- **FluentAssertions** - Assertions lisibles
- **AutoFixture** - Génération de données de test
- **WebApplicationFactory** - Tests d'intégration

## Structure des Tests

```
DotnetNiger.Community.Tests/
├── Unit/
│   ├── Services/
│   │   ├── PostServiceTests.cs
│   │   └── UserServiceTests.cs
│   ├── Validators/
│   │   └── CreatePostValidatorTests.cs
│   └── Mappers/
│       └── PostMapperTests.cs
├── Integration/
│   ├── Controllers/
│   │   └── PostsControllerTests.cs
│   ├── Repositories/
│   │   └── PostRepositoryTests.cs
│   └── Api/
│       └── PostApiTests.cs
└── TestHelpers/
    ├── Builders/
    ├── Fixtures/
    └── Mocks/
```

## Tests Unitaires

### Configuration

```csharp
public class PostServiceTests
{
    private readonly Mock<IPostRepository> _repositoryMock;
    private readonly Mock<ILogger<PostService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly PostService _sut; // System Under Test

    public PostServiceTests()
    {
        _repositoryMock = new Mock<IPostRepository>();
        _loggerMock = new Mock<ILogger<PostService>>();
        _mapperMock = new Mock<IMapper>();
        _sut = new PostService(_repositoryMock.Object, _loggerMock.Object, _mapperMock.Object);
    }
}
```

### Naming Convention

Format: `MethodName_Scenario_ExpectedBehavior`

```csharp
[Fact]
public async Task GetByIdAsync_WhenPostExists_ReturnsPost()
{
    // Arrange
    var postId = 1;
    var expectedPost = new Post { Id = postId, Title = "Test" };
    _repositoryMock.Setup(r => r.GetByIdAsync(postId))
        .ReturnsAsync(expectedPost);

    // Act
    var result = await _sut.GetByIdAsync(postId);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(postId);
    result.Title.Should().Be("Test");
}

[Fact]
public async Task GetByIdAsync_WhenPostDoesNotExist_ThrowsNotFoundException()
{
    // Arrange
    var postId = 999;
    _repositoryMock.Setup(r => r.GetByIdAsync(postId))
        .ReturnsAsync((Post)null);

    // Act
    Func<Task> act = async () => await _sut.GetByIdAsync(postId);

    // Assert
    await act.Should().ThrowAsync<NotFoundException>()
        .WithMessage($"Post with ID {postId} not found");
}
```

### Theory Tests (Data-Driven)

```csharp
[Theory]
[InlineData("")]
[InlineData(" ")]
[InlineData(null)]
public async Task CreateAsync_WhenTitleIsEmpty_ThrowsValidationException(string title)
{
    // Arrange
    var dto = new CreatePostDto { Title = title, Content = "Content" };

    // Act
    Func<Task> act = async () => await _sut.CreateAsync(dto);

    // Assert
    await act.Should().ThrowAsync<ValidationException>()
        .WithMessage("Title is required");
}

[Theory]
[MemberData(nameof(GetInvalidPostData))]
public async Task CreateAsync_WhenDataIsInvalid_ThrowsValidationException(CreatePostDto dto, string expectedMessage)
{
    // Act
    Func<Task> act = async () => await _sut.CreateAsync(dto);

    // Assert
    await act.Should().ThrowAsync<ValidationException>()
        .WithMessage(expectedMessage);
}

public static IEnumerable<object[]> GetInvalidPostData()
{
    yield return new object[] { new CreatePostDto { Title = "", Content = "Content" }, "Title is required" };
    yield return new object[] { new CreatePostDto { Title = "Title", Content = "" }, "Content is required" };
    yield return new object[] { new CreatePostDto { Title = new string('a', 201), Content = "Content" }, "Title too long" };
}
```

### Mocking

```csharp
// Setup simple
_repositoryMock.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new Post { Id = 1 });

// Setup avec arguments
_repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync((int id) => new Post { Id = id });

// Setup avec condition
_repositoryMock.Setup(r => r.GetByIdAsync(It.Is<int>(id => id > 0)))
    .ReturnsAsync(new Post());

// Setup pour exception
_repositoryMock.Setup(r => r.GetByIdAsync(999))
    .ThrowsAsync(new NotFoundException());

// Verify
_repositoryMock.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
_repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
```

### FluentAssertions

```csharp
// Objects
result.Should().NotBeNull();
result.Should().BeOfType<Post>();
result.Should().BeEquivalentTo(expectedPost);

// Collections
results.Should().NotBeEmpty();
results.Should().HaveCount(5);
results.Should().Contain(p => p.Id == 1);
results.Should().OnlyContain(p => p.IsActive);
results.Should().BeInAscendingOrder(p => p.CreatedAt);

// Strings
result.Title.Should().Be("Expected Title");
result.Title.Should().Contain("Test");
result.Title.Should().StartWith("Hello");
result.Title.Should().NotBeNullOrWhiteSpace();

// Numbers
result.Count.Should().BeGreaterThan(0);
result.Count.Should().BeLessThanOrEqualTo(10);

// Booleans
result.IsActive.Should().BeTrue();

// Exceptions
await act.Should().ThrowAsync<ValidationException>()
    .WithMessage("Validation failed")
    .Where(e => e.Errors.Count > 0);

// Dates
result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
result.CreatedAt.Should().BeAfter(startDate);
```

## Tests d'Intégration

### Configuration

```csharp
public class PostsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PostsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Build service provider
                var sp = services.BuildServiceProvider();

                // Seed test data
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                SeedTestData(context);
            });
        });

        _client = _factory.CreateClient();
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        context.Posts.AddRange(
            new Post { Id = 1, Title = "Test Post 1", Content = "Content 1" },
            new Post { Id = 2, Title = "Test Post 2", Content = "Content 2" }
        );
        context.SaveChanges();
    }
}
```

### API Tests

```csharp
[Fact]
public async Task GetPosts_ReturnsSuccessAndPosts()
{
    // Act
    var response = await _client.GetAsync("/api/posts");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    
    var content = await response.Content.ReadAsStringAsync();
    var posts = JsonSerializer.Deserialize<List<PostDto>>(content);
    
    posts.Should().NotBeNull();
    posts.Should().HaveCount(2);
}

[Fact]
public async Task CreatePost_WithValidData_ReturnsCreated()
{
    // Arrange
    var newPost = new CreatePostDto
    {
        Title = "New Post",
        Content = "New Content",
        UserId = 1
    };
    var json = JsonSerializer.Serialize(newPost);
    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

    // Act
    var response = await _client.PostAsync("/api/posts", httpContent);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    response.Headers.Location.Should().NotBeNull();
    
    var content = await response.Content.ReadAsStringAsync();
    var post = JsonSerializer.Deserialize<PostDto>(content);
    post.Title.Should().Be("New Post");
}

[Fact]
public async Task GetPost_WithInvalidId_ReturnsNotFound()
{
    // Act
    var response = await _client.GetAsync("/api/posts/999");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### Repository Tests

```csharp
public class PostRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PostRepository _repository;

    public PostRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PostRepository(_context);

        // Seed
        _context.Posts.AddRange(
            new Post { Id = 1, Title = "Post 1" },
            new Post { Id = 2, Title = "Post 2" }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_WhenPostExists_ReturnsPost()
    {
        // Act
        var post = await _repository.GetByIdAsync(1);

        // Assert
        post.Should().NotBeNull();
        post.Id.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_CreatesNewPost()
    {
        // Arrange
        var newPost = new Post { Title = "New Post", Content = "Content" };

        // Act
        var result = await _repository.AddAsync(newPost);
        await _context.SaveChangesAsync();

        // Assert
        result.Id.Should().BeGreaterThan(0);
        _context.Posts.Should().HaveCount(3);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

## Test Helpers

### Builders

```csharp
public class PostBuilder
{
    private int _id = 1;
    private string _title = "Default Title";
    private string _content = "Default Content";
    private int _userId = 1;

    public PostBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public PostBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PostBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public Post Build()
    {
        return new Post
        {
            Id = _id,
            Title = _title,
            Content = _content,
            UserId = _userId
        };
    }
}

// Usage
var post = new PostBuilder()
    .WithId(5)
    .WithTitle("Custom Title")
    .Build();
```

### AutoFixture

```csharp
public class PostServiceTests
{
    private readonly IFixture _fixture;

    public PostServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CreateAsync_CreatesPost()
    {
        // Arrange
        var dto = _fixture.Create<CreatePostDto>();
        
        // Act & Assert
        // ...
    }
}
```

## Couverture de Code

### Objectifs

- **Couverture minimale:** 80%
- **Services critiques:** 90%+
- **Controllers:** 80%+
- **Repositories:** 90%+

### Générer un Rapport

```bash
# Installer coverlet
dotnet add package coverlet.msbuild

# Run tests avec couverture
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Générer rapport HTML
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```

## Tests de Performance

```csharp
[Fact]
public async Task GetPosts_PerformanceTest()
{
    // Arrange
    var stopwatch = Stopwatch.StartNew();

    // Act
    var result = await _sut.GetAllAsync();
    stopwatch.Stop();

    // Assert
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
}
```

## Best Practices

### DO ✅

- Tester un seul comportement par test
- Utiliser AAA (Arrange, Act, Assert)
- Noms de tests explicites
- Tests indépendants (pas d'ordre requis)
- Mocker les dépendances externes
- Utiliser FluentAssertions
- Vérifier les cas limites (edge cases)
- Tester les exceptions
- Tests rapides (< 1s pour unitaires)

### DON'T ❌

- Tests dépendants les uns des autres
- Tests qui touchent la vraie DB
- Tests avec logique complexe
- Tests qui testent le framework
- Tests qui cassent facilement
- Ignorer les tests qui échouent
- Mocker ce qu'on teste
- Tests lents dans la suite unitaire

## Commandes

```bash
# Run tous les tests
dotnet test

# Run tests d'un projet spécifique
dotnet test DotnetNiger.Community.Tests

# Run tests avec filtre
dotnet test --filter "FullyQualifiedName~PostService"

# Run tests avec logger
dotnet test --logger "console;verbosity=detailed"

# Run avec couverture
dotnet test /p:CollectCoverage=true
```

## CI/CD

**GitHub Actions:**
```yaml
- name: Run Tests
  run: dotnet test --no-build --verbosity normal

- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage.cobertura.xml
```

---

**Dernière mise à jour:** 29 Janvier 2026
