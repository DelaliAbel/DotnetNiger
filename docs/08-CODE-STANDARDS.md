# 📏 Standards de Code

Standards et conventions de code pour DotnetNiger.

## Principes Généraux

### SOLID Principles

**S - Single Responsibility**

```csharp
// ❌ Mauvais
public class UserService
{
    public void CreateUser() { }
    public void SendEmail() { }
    public void ValidateToken() { }
}

// ✅ Bon
public class UserService
{
    public void CreateUser() { }
}

public class EmailService
{
    public void SendEmail() { }
}

public class TokenService
{
    public void ValidateToken() { }
}
```

**O - Open/Closed**

```csharp
// ✅ Ouvert à l'extension, fermé à la modification
public interface INotificationService
{
    Task SendAsync(string message);
}

public class EmailNotificationService : INotificationService
{
    public async Task SendAsync(string message) => await SendEmailAsync(message);
}

public class SmsNotificationService : INotificationService
{
    public async Task SendAsync(string message) => await SendSmsAsync(message);
}
```

**L - Liskov Substitution**

```csharp
// ✅ Les sous-classes doivent être substituables
public abstract class Repository<T>
{
    public abstract Task<T> GetByIdAsync(int id);
}

public class UserRepository : Repository<User>
{
    public override async Task<User> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }
}
```

**I - Interface Segregation**

```csharp
// ❌ Interface trop large
public interface IUserService
{
    Task Create();
    Task Update();
    Task Delete();
    Task SendEmail();
    Task GenerateReport();
}

// ✅ Interfaces spécifiques
public interface IUserCommandService
{
    Task CreateAsync();
    Task UpdateAsync();
    Task DeleteAsync();
}

public interface IUserNotificationService
{
    Task SendEmailAsync();
}

public interface IUserReportService
{
    Task GenerateReportAsync();
}
```

**D - Dependency Inversion**

```csharp
// ✅ Dépendre des abstractions
public class PostService
{
    private readonly IPostRepository _repository;
    private readonly ILogger<PostService> _logger;

    public PostService(IPostRepository repository, ILogger<PostService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

## Conventions de Nommage

### Classes et Interfaces

```csharp
// Classes: PascalCase
public class UserService { }
public class PostRepository { }

// Interfaces: I + PascalCase
public interface IUserService { }
public interface IPostRepository { }

// Abstract classes
public abstract class BaseRepository<T> { }
```

### Méthodes et Propriétés

```csharp
// Méthodes: PascalCase + verbe
public async Task<User> GetByIdAsync(int id) { }
public void UpdateEmail(string email) { }

// Propriétés: PascalCase
public string FirstName { get; set; }
public int Age { get; private set; }

// Propriétés booléennes: Is/Has/Can
public bool IsActive { get; set; }
public bool HasPermission { get; set; }
public bool CanEdit { get; set; }
```

### Variables et Paramètres

```csharp
// Variables locales: camelCase
var userId = 1;
var userName = "John";

// Paramètres: camelCase
public void UpdateUser(int userId, string userName) { }

// Champs privés: _camelCase
private readonly IUserRepository _userRepository;
private readonly ILogger<UserService> _logger;
```

### Constants et Enums

```csharp
// Constants: PascalCase
public const int MaxRetryCount = 3;
public const string DefaultCulture = "fr-NE";

// Enums: PascalCase
public enum UserRole
{
    Admin,
    Moderator,
    Member
}
```

## Structure de Fichiers

### Organisation

```csharp
// 1. Usings (triés)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DotnetNiger.Community.Domain.Entities;

// 2. Namespace
namespace DotnetNiger.Community.Application.Services;

// 3. Class documentation
/// <summary>
/// Service for managing posts
/// </summary>
public class PostService
{
    // 4. Private fields
    private readonly IPostRepository _repository;
    private readonly ILogger<PostService> _logger;

    // 5. Constructor
    public PostService(IPostRepository repository, ILogger<PostService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // 6. Public methods
    public async Task<Post> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // 7. Private methods
    private void ValidatePost(Post post)
    {
        // Validation logic
    }
}
```

## Clean Code

### Méthodes

```csharp
// ✅ Courtes et focalisées (< 20 lignes idéalement)
public async Task<PostDto> CreateAsync(CreatePostDto dto)
{
    ValidateDto(dto);
    var post = MapToEntity(dto);
    await _repository.AddAsync(post);
    await _unitOfWork.SaveChangesAsync();
    return MapToDto(post);
}

// ✅ Pas plus de 3 paramètres
public void UpdatePost(int id, string title, string content) { }

// ❌ Trop de paramètres
public void UpdatePost(int id, string title, string content, string category,
    DateTime date, bool isPublished, string tags, string author) { }

// ✅ Utiliser un DTO
public void UpdatePost(int id, UpdatePostDto dto) { }
```

### Variables

```csharp
// ✅ Noms explicites
var activeUsers = await GetActiveUsersAsync();
var totalCount = posts.Count();

// ❌ Noms cryptiques
var au = await GetAuAsync();
var tc = p.Count();

// ✅ Pas de magic numbers
private const int MaxPostTitleLength = 200;
if (title.Length > MaxPostTitleLength)
    throw new ValidationException("Title too long");

// ❌ Magic numbers
if (title.Length > 200)
    throw new ValidationException("Title too long");
```

### Commentaires

```csharp
// ✅ XML comments pour APIs publiques
/// <summary>
/// Creates a new post
/// </summary>
/// <param name="dto">Post data</param>
/// <returns>Created post</returns>
public async Task<PostDto> CreateAsync(CreatePostDto dto)

// ✅ Expliquer le "pourquoi", pas le "quoi"
// Waiting 1 second to avoid rate limiting
await Task.Delay(1000);

// ❌ Commentaires inutiles
// Increment counter
counter++;
```

## Gestion des Erreurs

### Exceptions

```csharp
// ✅ Exceptions spécifiques
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

// ✅ Try-catch approprié
public async Task<Post> GetByIdAsync(int id)
{
    try
    {
        return await _repository.GetByIdAsync(id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching post {PostId}", id);
        throw;
    }
}

// ❌ Catch vide
try
{
    // Code
}
catch
{
    // Rien
}
```

### Validation

```csharp
// ✅ Guard clauses en début de méthode
public async Task<Post> UpdateAsync(int id, UpdatePostDto dto)
{
    if (id <= 0)
        throw new ArgumentException("Invalid ID", nameof(id));

    if (dto == null)
        throw new ArgumentNullException(nameof(dto));

    if (string.IsNullOrWhiteSpace(dto.Title))
        throw new ValidationException("Title is required");

    // Business logic
}
```

## Async/Await

```csharp
// ✅ Méthodes async suffixées par Async
public async Task<User> GetUserAsync(int id) { }

// ✅ ConfigureAwait(false) dans libraries
public async Task<User> GetUserAsync(int id)
{
    return await _repository.GetByIdAsync(id).ConfigureAwait(false);
}

// ✅ Pas de async void (sauf event handlers)
public async Task ProcessAsync() { }

// ❌ Éviter
public async void ProcessAsync() { }

// ✅ ValueTask pour hot paths
public ValueTask<bool> ExistsAsync(int id)
{
    if (_cache.Contains(id))
        return new ValueTask<bool>(true);

    return ExistsInDbAsync(id);
}
```

## Dependency Injection

```csharp
// ✅ Enregistrement par interface
services.AddScoped<IUserService, UserService>();
services.AddScoped<IPostRepository, PostRepository>();

// ✅ Injection par constructeur
public class PostService
{
    private readonly IPostRepository _repository;

    public PostService(IPostRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}

// ❌ Service Locator anti-pattern
var service = serviceProvider.GetService<IUserService>();
```

## Entity Framework Core

```csharp
// ✅ Async queries
var users = await _context.Users
    .Where(u => u.IsActive)
    .ToListAsync();

// ✅ Projection
var userDtos = await _context.Users
    .Select(u => new UserDto
    {
        Id = u.Id,
        Name = u.Name
    })
    .ToListAsync();

// ❌ N+1 queries
foreach (var post in posts)
{
    var author = await _context.Users.FindAsync(post.AuthorId);
}

// ✅ Include
var posts = await _context.Posts
    .Include(p => p.Author)
    .ToListAsync();

// ✅ AsNoTracking pour read-only
var users = await _context.Users
    .AsNoTracking()
    .ToListAsync();
```

## Sécurité

```csharp
// ✅ Parameterized queries (EF Core le fait automatiquement)
var users = await _context.Users
    .Where(u => u.Email == email)
    .ToListAsync();

// ❌ String concatenation SQL (never!)
// var sql = $"SELECT * FROM Users WHERE Email = '{email}'";

// ✅ Valider les entrées
public void UpdateEmail(string email)
{
    if (!IsValidEmail(email))
        throw new ValidationException("Invalid email");

    // Update logic
}

// ✅ Pas de secrets dans le code
// ❌
var apiKey = "sk_test_1234567890";

// ✅
var apiKey = _configuration["SendGrid:ApiKey"];
```

## Performance

```csharp
// ✅ StringBuilder pour concatenation
var builder = new StringBuilder();
foreach (var item in items)
{
    builder.AppendLine(item.ToString());
}

// ✅ LINQ optimisé
var count = collection.Count(x => x.IsActive);
// au lieu de
// var count = collection.Where(x => x.IsActive).Count();

// ✅ Caching approprié
var cachedValue = await _cache.GetOrCreateAsync("key", async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    return await ExpensiveOperationAsync();
});
```

## Code Reviews

### Checklist

- [ ] Code suit les conventions de nommage
- [ ] Principes SOLID respectés
- [ ] Gestion d'erreurs appropriée
- [ ] Tests unitaires inclus
- [ ] Pas de code dupliqué
- [ ] Async/await correctement utilisé
- [ ] Performance acceptable
- [ ] Sécurité validée
- [ ] Documentation XML pour APIs publiques

---

**Dernière mise à jour:** 29 Janvier 2026
