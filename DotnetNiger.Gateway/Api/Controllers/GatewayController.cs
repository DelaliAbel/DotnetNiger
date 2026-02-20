using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Gateway.Infrastructure.HttpClients;
using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly IIdentityApiClient _identityClient;
    private readonly ICommunityApiClient _communityClient;
    private readonly IRouteService _routeService;
    private readonly ICachingService _cachingService;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(
        IIdentityApiClient identityClient,
        ICommunityApiClient communityClient,
        IRouteService routeService,
        ICachingService cachingService,
        IMetricsService metricsService,
        ILogger<GatewayController> logger)
    {
        _identityClient = identityClient;
        _communityClient = communityClient;
        _routeService = routeService;
        _cachingService = cachingService;
        _metricsService = metricsService;
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        _metricsService.RecordRequest("/api/gateway/status", HttpContext.Request.Method);
        
        return Ok(new
        {
            status = "Gateway is running",
            timestamp = DateTime.UtcNow,
            services = new
            {
                identity = "Available at /identity",
                community = "Available at /community",
                swagger = "Available at /swagger"
            }
        });
    }

    [HttpGet("identity/users/me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        _metricsService.RecordRequest("/api/gateway/identity/users/me", HttpContext.Request.Method);

        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Token not provided");

            var user = await _identityClient.GetCurrentUserAsync(token);
            
            if (user == null)
                return Unauthorized("Invalid token");

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            _metricsService.RecordError("/api/gateway/identity/users/me", ex.GetType().Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("identity/auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _metricsService.RecordRequest("/api/gateway/identity/auth/login", HttpContext.Request.Method);

        try
        {
            var result = await _identityClient.AuthenticateAsync(new DotnetNiger.Gateway.Infrastructure.HttpClients.LoginRequest
            {
                Email = request.Email,
                Password = request.Password
            });

            if (result == null)
                return Unauthorized("Invalid credentials");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            _metricsService.RecordError("/api/gateway/identity/auth/login", ex.GetType().Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("community/posts")]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _metricsService.RecordRequest("/api/gateway/community/posts", HttpContext.Request.Method);

        try
        {
            var cacheKey = $"posts:page:{page}:size:{pageSize}";
            
            // Vérifier le cache
            var cachedPosts = await _cachingService.GetAsync<List<PostResponse>>(cacheKey);
            if (cachedPosts != null)
            {
                _logger.LogInformation("Returning posts from cache");
                return Ok(new { source = "cache", posts = cachedPosts });
            }

            // Récupérer depuis le service
            var posts = await _communityClient.GetPostsAsync(page, pageSize);
            
            if (posts == null)
                return NotFound("No posts found");

            // Mettre en cache
            await _cachingService.SetAsync(cacheKey, posts, TimeSpan.FromMinutes(10));

            return Ok(new { source = "service", posts = posts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts");
            _metricsService.RecordError("/api/gateway/community/posts", ex.GetType().Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("community/events")]
    public async Task<IActionResult> GetEvents()
    {
        _metricsService.RecordRequest("/api/gateway/community/events", HttpContext.Request.Method);

        try
        {
            var cacheKey = "events:all";
            
            // Vérifier le cache
            var cachedEvents = await _cachingService.GetAsync<List<EventResponse>>(cacheKey);
            if (cachedEvents != null)
            {
                _logger.LogInformation("Returning events from cache");
                return Ok(new { source = "cache", events = cachedEvents });
            }

            // Récupérer depuis le service
            var events = await _communityClient.GetEventsAsync();
            
            if (events == null)
                return NotFound("No events found");

            // Mettre en cache
            await _cachingService.SetAsync(cacheKey, events, TimeSpan.FromMinutes(15));

            return Ok(new { source = "service", events = events });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events");
            _metricsService.RecordError("/api/gateway/community/events", ex.GetType().Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("community/projects")]
    public async Task<IActionResult> GetProjects()
    {
        _metricsService.RecordRequest("/api/gateway/community/projects", HttpContext.Request.Method);

        try
        {
            var projects = await _communityClient.GetProjectsAsync();
            
            if (projects == null)
                return NotFound("No projects found");

            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            _metricsService.RecordError("/api/gateway/community/projects", ex.GetType().Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        _metricsService.RecordRequest("/api/gateway/search", HttpContext.Request.Method);

        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query parameter is required");

        try
        {
            var cacheKey = $"search:{query}";
            
            // Vérifier le cache
            var cachedResults = await _cachingService.GetAsync<SearchResponse>(cacheKey);
            if (cachedResults != null)
            {
                _logger.LogInformation("Returning search results from cache");
                return Ok(new { source = "cache", results = cachedResults });
            }

            // Récupérer depuis le service
            var results = await _communityClient.SearchAsync(query);
            
            if (results == null)
                return NotFound("No results found");

            // Mettre en cache
            await _cachingService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(5));

            return Ok(new { source = "service", results = results });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching");
            _metricsService.RecordError("/api/gateway/search", ex.GetType().Name);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("stats")]
    public IActionResult GetStatistics()
    {
        return Ok(new
        {
            message = "Statistics endpoint",
            gateway = "Monitoring gateway performance and microservice health",
            endpoints = new
            {
                identity = "/identity",
                community = "/community",
                swagger = "/swagger"
            }
        });
    }
}

/// <summary>
/// Modèle de requête pour la connexion
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
