namespace DotnetNiger.Gateway.Infrastructure.HttpClients;

/// <summary>
/// Interface pour le client API Community Service
/// </summary>
public interface ICommunityApiClient
{
    /// <summary>
    /// Récupère tous les posts
    /// </summary>
    Task<List<PostResponse>?> GetPostsAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Récupère un post par ID
    /// </summary>
    Task<PostResponse?> GetPostByIdAsync(string postId);

    /// <summary>
    /// Crée un nouveau post
    /// </summary>
    Task<PostResponse?> CreatePostAsync(CreatePostRequest request);

    /// <summary>
    /// Récupère tous les événements
    /// </summary>
    Task<List<EventResponse>?> GetEventsAsync();

    /// <summary>
    /// Récupère un événement par ID
    /// </summary>
    Task<EventResponse?> GetEventByIdAsync(string eventId);

    /// <summary>
    /// Crée un nouvel événement
    /// </summary>
    Task<EventResponse?> CreateEventAsync(CreateEventRequest request);

    /// <summary>
    /// Récupère les projets
    /// </summary>
    Task<List<ProjectResponse>?> GetProjectsAsync();

    /// <summary>
    /// Récupère un projet par ID
    /// </summary>
    Task<ProjectResponse?> GetProjectByIdAsync(string projectId);

    /// <summary>
    /// Récupère les catégories
    /// </summary>
    Task<List<CategoryResponse>?> GetCategoriesAsync();

    /// <summary>
    /// Récupère les ressources
    /// </summary>
    Task<List<ResourceResponse>?> GetResourcesAsync();

    /// <summary>
    /// Recherche du contenu
    /// </summary>
    Task<SearchResponse?> SearchAsync(string query);
}

/// <summary>
/// Données de réponse pour un post
/// </summary>
public class PostResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
}

/// <summary>
/// Données de réponse pour un événement
/// </summary>
public class EventResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Attendees { get; set; }
}

/// <summary>
/// Données de réponse pour un projet
/// </summary>
public class ProjectResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> TeamMembers { get; set; } = new();
}

/// <summary>
/// Données de réponse pour une catégorie
/// </summary>
public class CategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

/// <summary>
/// Données de réponse pour une ressource
/// </summary>
public class ResourceResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Données de réponse pour la recherche
/// </summary>
public class SearchResponse
{
    public List<PostResponse> Posts { get; set; } = new();
    public List<EventResponse> Events { get; set; } = new();
    public List<ProjectResponse> Projects { get; set; } = new();
    public int TotalResults { get; set; }
}

/// <summary>
/// Données de requête pour créer un post
/// </summary>
public class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Données de requête pour créer un événement
/// </summary>
public class CreateEventRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}
