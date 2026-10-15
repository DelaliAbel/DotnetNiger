using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Profil public d'un membre.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug du profil.</param>
/// <param name="FullName">Nom complet.</param>
/// <param name="Bio">Biographie.</param>
/// <param name="City">Ville.</param>
/// <param name="PhotoUrl">Photo de profil.</param>
/// <param name="Technologies">Compétences déclarées.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
public sealed record MemberResponse(
    Guid Id,
    string? Slug,
    string FullName,
    string? Bio,
    string? City,
    string? PhotoUrl,
    string? Technologies,
    bool IsTeam);

/// <summary>Demande de mise à jour du profil du membre connecté.</summary>
/// <param name="FirstName">Prénom.</param>
/// <param name="LastName">Nom.</param>
/// <param name="Bio">Biographie.</param>
/// <param name="City">Ville.</param>
/// <param name="PhotoUrl">Photo de profil.</param>
/// <param name="GitHubUsername">Compte GitHub.</param>
/// <param name="LinkedInUrl">Profil LinkedIn.</param>
/// <param name="XUrl">Profil X.</param>
/// <param name="Technologies">Compétences déclarées.</param>
public sealed record UpdateProfileRequest(
    [Required, StringLength(150)] string FirstName,
    [Required, StringLength(150)] string LastName,
    [StringLength(2000)] string? Bio = null,
    [StringLength(150)] string? City = null,
    [StringLength(2048)] string? PhotoUrl = null,
    [StringLength(100)] string? GitHubUsername = null,
    [StringLength(2048)] string? LinkedInUrl = null,
    [StringLength(2048)] string? XUrl = null,
    [StringLength(300)] string? Technologies = null);

