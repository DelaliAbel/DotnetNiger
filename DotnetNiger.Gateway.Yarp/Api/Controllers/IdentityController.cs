using Microsoft.AspNetCore.Mvc;
using DotnetNiger.Gateway.Infrastructure.HttpClients;
using DotnetNiger.Gateway.Application.Services.Interfaces;

namespace DotnetNiger.Gateway.Api.Controllers;

/// <summary>
/// Contrôleur pour les endpoints du service Identity
/// Les clients consomment via /api/identity/*
/// </summary>
[ApiController]
[Route("api/identity")]
public class IdentityController : ControllerBase
{
    private readonly IIdentityApiClient _identityClient;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<IdentityController> _logger;

    public IdentityController(
        IIdentityApiClient identityClient,
        IMetricsService metricsService,
        ILogger<IdentityController> logger)
    {
        _identityClient = identityClient;
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Authentifier un utilisateur
    /// </summary>
    /// <param name="request">Identifiants de connexion</param>
    /// <returns>Token d'accès et informations utilisateur</returns>
    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        _metricsService.RecordRequest("/api/identity/auth/login", HttpContext.Request.Method);

        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Email et mot de passe sont requis" });

        try
        {
            var result = await _identityClient.AuthenticateAsync(new Infrastructure.HttpClients.LoginRequest
            {
                Email = request.Email,
                Password = request.Password
            });

            if (result == null)
                return Unauthorized(new { message = "Identifiants invalides" });

            _logger.LogInformation($"Utilisateur connecté: {request.Email}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            _metricsService.RecordError("/api/identity/auth/login", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la connexion" });
        }
    }

    /// <summary>
    /// Enregistrer un nouvel utilisateur
    /// </summary>
    /// <param name="request">Données d'enregistrement</param>
    /// <returns>Token d'accès et informations utilisateur</returns>
    [HttpPost("auth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        _metricsService.RecordRequest("/api/identity/auth/register", HttpContext.Request.Method);

        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Email et mot de passe sont requis" });

        try
        {
            var result = await _identityClient.RegisterAsync(new Infrastructure.HttpClients.RegisterRequest
            {
                Email = request.Email,
                Password = request.Password,
                Username = request.Username,
                FullName = request.FullName
            });

            if (result == null)
                return BadRequest(new { message = "L'enregistrement a échoué" });

            _logger.LogInformation($"Nouvel utilisateur enregistré: {request.Email}");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement");
            _metricsService.RecordError("/api/identity/auth/register", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de l'enregistrement" });
        }
    }

    /// <summary>
    /// Obtenir les informations de l'utilisateur actuel
    /// </summary>
    /// <returns>Informations utilisateur</returns>
    [HttpGet("users/me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        _metricsService.RecordRequest("/api/identity/users/me", HttpContext.Request.Method);

        try
        {
            var token = ExtractToken(HttpContext.Request.Headers["Authorization"]);
            
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Token manquant" });

            var user = await _identityClient.GetCurrentUserAsync(token);
            
            if (user == null)
                return Unauthorized(new { message = "Token invalide" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur");
            _metricsService.RecordError("/api/identity/users/me", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération de l'utilisateur" });
        }
    }

    /// <summary>
    /// Obtenir un utilisateur par ID
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <returns>Informations utilisateur</returns>
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        _metricsService.RecordRequest($"/api/identity/users/{userId}", HttpContext.Request.Method);

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { message = "ID utilisateur requis" });

        try
        {
            var user = await _identityClient.GetUserByIdAsync(userId);
            
            if (user == null)
                return NotFound(new { message = "Utilisateur non trouvé" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération de l'utilisateur {userId}");
            _metricsService.RecordError($"/api/identity/users/{userId}", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération de l'utilisateur" });
        }
    }

    /// <summary>
    /// Obtenir tous les rôles disponibles
    /// </summary>
    /// <returns>Liste des rôles</returns>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        _metricsService.RecordRequest("/api/identity/roles", HttpContext.Request.Method);

        try
        {
            var roles = await _identityClient.GetRolesAsync();
            
            if (roles == null)
                return NotFound(new { message = "Aucun rôle trouvé" });

            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des rôles");
            _metricsService.RecordError("/api/identity/roles", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la récupération des rôles" });
        }
    }

    /// <summary>
    /// Rafraîchir le token d'accès
    /// </summary>
    /// <param name="request">Token de rafraîchissement</param>
    /// <returns>Nouveau token d'accès</returns>
    [HttpPost("tokens/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        _metricsService.RecordRequest("/api/identity/tokens/refresh", HttpContext.Request.Method);

        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(new { message = "Refresh token requis" });

        try
        {
            var result = await _identityClient.RefreshTokenAsync(request.RefreshToken);
            
            if (result == null)
                return Unauthorized(new { message = "Refresh token invalide" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
            _metricsService.RecordError("/api/identity/tokens/refresh", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors du rafraîchissement du token" });
        }
    }

    /// <summary>
    /// Valider un token JWT
    /// </summary>
    /// <param name="request">Token à valider</param>
    /// <returns>Résultat de la validation</returns>
    [HttpPost("tokens/validate")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequestDto request)
    {
        _metricsService.RecordRequest("/api/identity/tokens/validate", HttpContext.Request.Method);

        if (string.IsNullOrEmpty(request.Token))
            return BadRequest(new { message = "Token requis" });

        try
        {
            var isValid = await _identityClient.ValidateTokenAsync(request.Token);
            
            return Ok(new { valid = isValid, message = isValid ? "Token valide" : "Token invalide" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation du token");
            _metricsService.RecordError("/api/identity/tokens/validate", ex.GetType().Name);
            return StatusCode(500, new { message = "Erreur lors de la validation du token" });
        }
    }

    /// <summary>
    /// Extraire le token du header Authorization
    /// </summary>
    private string? ExtractToken(string? authHeader)
    {
        if (string.IsNullOrEmpty(authHeader))
            return null;

        const string bearerScheme = "Bearer ";
        return authHeader.StartsWith(bearerScheme)
            ? authHeader.Substring(bearerScheme.Length)
            : null;
    }
}

/// <summary>
/// DTO pour la login
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour l'enregistrement
/// </summary>
public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour le rafraîchissement du token
/// </summary>
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour la validation du token
/// </summary>
public class ValidateTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
}
