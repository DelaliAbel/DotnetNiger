using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Gateway.Infrastructure.HttpClients;
using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Api.Controllers;

/// <summary>
/// Contrôleur pour les endpoints du service Community
/// Les clients consomment via /api/community/*
/// </summary>
[ApiController]
[Route("api/community")]
public class CommunityController : ControllerBase
{
    private readonly ICommunityApiClient _communityClient;
    private readonly ICachingService _cachingService;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<CommunityController> _logger;

    public CommunityController(
        ICommunityApiClient communityClient,
        ICachingService cachingService,
        IMetricsService metricsService,
        ILogger<CommunityController> logger)
    {
        _communityClient = communityClient;
        _cachingService = cachingService;
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Récupérer tous les posts avec pagination
    /// </summary>
    /// <param name="page">Numéro de page (par défaut 1)</param>
    /// <param name="pageSize">Nombre de posts par page (par défaut 10)</param>
    /// <returns>Liste paginée des posts</returns>
    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _metricsService.RecordRequest("/api/community/posts", HttpContext.Request.Method);

        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Paramètres de pagination invalides" });

        try
        {
            var cacheKey = $"community:posts:page:{page}:size:{pageSize}";
            
            // Vérifier le cache
            var cachedPosts = await _cachingService.GetAsync<List<PostResponse>>(cacheKey);
            if (cachedPosts != null)
            {
                _logger.LogInformation("Posts retournés depuis le cache");
                return Ok(new
                {
                    source = "cache",
                    page = page,
                    pageSize = pageSize,
                    data = cachedPosts
                });
            }

            // Récupérer depuis le service
            var posts = await _communityClient.GetPostsAsync(page, pageSize);
            
            if (posts == null || posts.Count == 0)
                return NotFound(new { message = "Aucun post trouvé" });

            // Mettre en cache
            await _cachingService.SetAsync(cacheKey, posts, TimeSpan.FromMinutes(10));

            return Ok(new
            {
                source = "service",
                page = page,
                pageSize = pageSize,
                data = posts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des posts");
            _metricsService.RecordError("/api/community/posts", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération des posts" });
        }
    }

    /// <summary>
    /// Récupérer un post par ID
    /// </summary>
    /// <param name="postId">ID du post</param>
    /// <returns>Détails du post</returns>
    [HttpGet("posts/{postId}")]
    public async Task<IActionResult> GetPostById(string postId)
    {
        _metricsService.RecordRequest($"/api/community/posts/{postId}", HttpContext.Request.Method);

        if (string.IsNullOrEmpty(postId))
            return BadRequest(new { message = "ID du post requis" });

        try
        {
            var cacheKey = $"community:post:{postId}";
            
            var cachedPost = await _cachingService.GetAsync<PostResponse>(cacheKey);
            if (cachedPost != null)
                return Ok(new { source = "cache", data = cachedPost });

            var post = await _communityClient.GetPostByIdAsync(postId);
            
            if (post == null)
                return NotFound(new { message = "Post non trouvé" });

            await _cachingService.SetAsync(cacheKey, post, TimeSpan.FromMinutes(30));

            return Ok(new { source = "service", data = post });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération du post {postId}");
            _metricsService.RecordError($"/api/community/posts/{postId}", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération du post" });
        }
    }

    /// <summary>
    /// Créer un nouveau post
    /// </summary>
    /// <param name="request">Données du post</param>
    /// <returns>Post créé</returns>
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequestDto request)
    {
        _metricsService.RecordRequest("/api/community/posts", "POST");

        if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Content))
            return BadRequest(new { message = "Titre et contenu sont requis" });

        try
        {
            var post = await _communityClient.CreatePostAsync(new CreatePostRequest
            {
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags ?? new List<string>()
            });

            if (post == null)
                return BadRequest(new { message = "La création du post a échoué" });

            // Invalider le cache
            await InvalidatePostsCache();

            _logger.LogInformation("Nouveau post créé");
            return CreatedAtAction(nameof(GetPostById), new { postId = post.Id }, post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du post");
            _metricsService.RecordError("/api/community/posts", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la création du post" });
        }
    }

    /// <summary>
    /// Récupérer tous les événements
    /// </summary>
    /// <returns>Liste des événements</returns>
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents()
    {
        _metricsService.RecordRequest("/api/community/events", HttpContext.Request.Method);

        try
        {
            var cacheKey = "community:events:all";
            
            var cachedEvents = await _cachingService.GetAsync<List<EventResponse>>(cacheKey);
            if (cachedEvents != null)
                return Ok(new { source = "cache", data = cachedEvents });

            var events = await _communityClient.GetEventsAsync();
            
            if (events == null || events.Count == 0)
                return NotFound(new { message = "Aucun événement trouvé" });

            await _cachingService.SetAsync(cacheKey, events, TimeSpan.FromMinutes(15));

            return Ok(new { source = "service", data = events });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des événements");
            _metricsService.RecordError("/api/community/events", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération des événements" });
        }
    }

    /// <summary>
    /// Récupérer un événement par ID
    /// </summary>
    /// <param name="eventId">ID de l'événement</param>
    /// <returns>Détails de l'événement</returns>
    [HttpGet("events/{eventId}")]
    public async Task<IActionResult> GetEventById(string eventId)
    {
        _metricsService.RecordRequest($"/api/community/events/{eventId}", HttpContext.Request.Method);

        if (string.IsNullOrEmpty(eventId))
            return BadRequest(new { message = "ID de l'événement requis" });

        try
        {
            var cacheKey = $"community:event:{eventId}";
            
            var cachedEvent = await _cachingService.GetAsync<EventResponse>(cacheKey);
            if (cachedEvent != null)
                return Ok(new { source = "cache", data = cachedEvent });

            var eventData = await _communityClient.GetEventByIdAsync(eventId);
            
            if (eventData == null)
                return NotFound(new { message = "Événement non trouvé" });

            await _cachingService.SetAsync(cacheKey, eventData, TimeSpan.FromMinutes(30));

            return Ok(new { source = "service", data = eventData });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération de l'événement {eventId}");
            _metricsService.RecordError($"/api/community/events/{eventId}", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération de l'événement" });
        }
    }

    /// <summary>
    /// Créer un nouvel événement
    /// </summary>
    /// <param name="request">Données de l'événement</param>
    /// <returns>Événement créé</returns>
    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequestDto request)
    {
        _metricsService.RecordRequest("/api/community/events", "POST");

        if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Location))
            return BadRequest(new { message = "Titre et localisation sont requis" });

        try
        {
            var eventData = await _communityClient.CreateEventAsync(new CreateEventRequest
            {
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location,
                Tags = request.Tags ?? new List<string>()
            });

            if (eventData == null)
                return BadRequest(new { message = "La création de l'événement a échoué" });

            await InvalidateEventsCache();

            _logger.LogInformation("Nouvel événement créé");
            return CreatedAtAction(nameof(GetEventById), new { eventId = eventData.Id }, eventData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'événement");
            _metricsService.RecordError("/api/community/events", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la création de l'événement" });
        }
    }

    /// <summary>
    /// Récupérer tous les projets
    /// </summary>
    /// <returns>Liste des projets</returns>
    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects()
    {
        _metricsService.RecordRequest("/api/community/projects", HttpContext.Request.Method);

        try
        {
            var cacheKey = "community:projects:all";
            
            var cachedProjects = await _cachingService.GetAsync<List<ProjectResponse>>(cacheKey);
            if (cachedProjects != null)
                return Ok(new { source = "cache", data = cachedProjects });

            var projects = await _communityClient.GetProjectsAsync();
            
            if (projects == null || projects.Count == 0)
                return NotFound(new { message = "Aucun projet trouvé" });

            await _cachingService.SetAsync(cacheKey, projects, TimeSpan.FromMinutes(20));

            return Ok(new { source = "service", data = projects });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des projets");
            _metricsService.RecordError("/api/community/projects", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération des projets" });
        }
    }

    /// <summary>
    /// Récupérer un projet par ID
    /// </summary>
    /// <param name="projectId">ID du projet</param>
    /// <returns>Détails du projet</returns>
    [HttpGet("projects/{projectId}")]
    public async Task<IActionResult> GetProjectById(string projectId)
    {
        _metricsService.RecordRequest($"/api/community/projects/{projectId}", HttpContext.Request.Method);

        if (string.IsNullOrEmpty(projectId))
            return BadRequest(new { message = "ID du projet requis" });

        try
        {
            var cacheKey = $"community:project:{projectId}";
            
            var cachedProject = await _cachingService.GetAsync<ProjectResponse>(cacheKey);
            if (cachedProject != null)
                return Ok(new { source = "cache", data = cachedProject });

            var project = await _communityClient.GetProjectByIdAsync(projectId);
            
            if (project == null)
                return NotFound(new { message = "Projet non trouvé" });

            await _cachingService.SetAsync(cacheKey, project, TimeSpan.FromMinutes(30));

            return Ok(new { source = "service", data = project });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération du projet {projectId}");
            _metricsService.RecordError($"/api/community/projects/{projectId}", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération du projet" });
        }
    }

    /// <summary>
    /// Récupérer toutes les catégories
    /// </summary>
    /// <returns>Liste des catégories</returns>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        _metricsService.RecordRequest("/api/community/categories", HttpContext.Request.Method);

        try
        {
            var cacheKey = "community:categories:all";
            
            var cachedCategories = await _cachingService.GetAsync<List<CategoryResponse>>(cacheKey);
            if (cachedCategories != null)
                return Ok(new { source = "cache", data = cachedCategories });

            var categories = await _communityClient.GetCategoriesAsync();
            
            if (categories == null || categories.Count == 0)
                return NotFound(new { message = "Aucune catégorie trouvée" });

            await _cachingService.SetAsync(cacheKey, categories, TimeSpan.FromHours(1));

            return Ok(new { source = "service", data = categories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des catégories");
            _metricsService.RecordError("/api/community/categories", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération des catégories" });
        }
    }

    /// <summary>
    /// Récupérer toutes les ressources
    /// </summary>
    /// <returns>Liste des ressources</returns>
    [HttpGet("resources")]
    public async Task<IActionResult> GetResources()
    {
        _metricsService.RecordRequest("/api/community/resources", HttpContext.Request.Method);

        try
        {
            var cacheKey = "community:resources:all";
            
            var cachedResources = await _cachingService.GetAsync<List<ResourceResponse>>(cacheKey);
            if (cachedResources != null)
                return Ok(new { source = "cache", data = cachedResources });

            var resources = await _communityClient.GetResourcesAsync();
            
            if (resources == null || resources.Count == 0)
                return NotFound(new { message = "Aucune ressource trouvée" });

            await _cachingService.SetAsync(cacheKey, resources, TimeSpan.FromHours(2));

            return Ok(new { source = "service", data = resources });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des ressources");
            _metricsService.RecordError("/api/community/resources", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération des ressources" });
        }
    }

    /// <summary>
    /// Rechercher du contenu à travers le service Community
    /// </summary>
    /// <param name="query">Terme de recherche</param>
    /// <returns>Résultats de la recherche</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        _metricsService.RecordRequest("/api/community/search", HttpContext.Request.Method);

        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Terme de recherche requis" });

        try
        {
            var cacheKey = $"community:search:{query.ToLower()}";
            
            var cachedResults = await _cachingService.GetAsync<SearchResponse>(cacheKey);
            if (cachedResults != null)
                return Ok(new { source = "cache", data = cachedResults });

            var results = await _communityClient.SearchAsync(query);
            
            if (results == null)
                return NotFound(new { message = "Aucun résultat trouvé" });

            await _cachingService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5));

            return Ok(new { source = "service", data = results });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la recherche: {query}");
            _metricsService.RecordError("/api/community/search", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la recherche" });
        }
    }

    /// <summary>
    /// Invalider le cache des posts pour la liste personnalisée
    /// </summary>
    private async Task InvalidatePostsCache()
    {
        // Invalider plusieurs clés de cache pour les posts
        for (int page = 1; page <= 5; page++)
        {
            for (int size = 10; size <= 100; size += 10)
            {
                await _cachingService.RemoveAsync($"community:posts:page:{page}:size:{size}");
            }
        }
    }

    /// <summary>
    /// Invalider le cache des événements
    /// </summary>
    private async Task InvalidateEventsCache()
    {
        await _cachingService.RemoveAsync("community:events:all");
    }
}

/// <summary>
/// DTO pour la création de post
/// </summary>
public class CreatePostRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
}

/// <summary>
/// DTO pour la création d'événement
/// </summary>
public class CreateEventRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
}
