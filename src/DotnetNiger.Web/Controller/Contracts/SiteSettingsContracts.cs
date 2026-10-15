using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>Réglages du site tels que l'administration les modifie.</summary>
/// <param name="Logo">Logo du site, en-tête et courriels, ou <see langword="null"/>.</param>
/// <param name="Email">Adresse de contact, ou <see langword="null"/>.</param>
/// <param name="WhatsappUrl">Adresse du groupe WhatsApp, ou <see langword="null"/>.</param>
/// <param name="GithubUrl">Dépôt GitHub, ou <see langword="null"/>.</param>
/// <param name="LinkedinUrl">Page LinkedIn, ou <see langword="null"/>.</param>
/// <param name="XUrl">Compte X, ou <see langword="null"/>.</param>
/// <param name="YoutubeUrl">Chaîne YouTube, ou <see langword="null"/>.</param>
/// <param name="FacebookUrl">Page Facebook, ou <see langword="null"/>.</param>
public sealed record SiteSettingsResponse(
    string? Logo,
    string? Email,
    string? WhatsappUrl,
    string? GithubUrl,
    string? LinkedinUrl,
    string? XUrl,
    string? YoutubeUrl,
    string? FacebookUrl);

/// <summary>Demande de modification des réglages du site.</summary>
/// <param name="Logo">Logo du site, ou <see langword="null"/> pour le retirer.</param>
/// <param name="Email">Adresse de contact, ou <see langword="null"/> pour la retirer.</param>
/// <param name="WhatsappUrl">Adresse du groupe WhatsApp, ou <see langword="null"/>.</param>
/// <param name="GithubUrl">Dépôt GitHub, ou <see langword="null"/>.</param>
/// <param name="LinkedinUrl">Page LinkedIn, ou <see langword="null"/>.</param>
/// <param name="XUrl">Compte X, ou <see langword="null"/>.</param>
/// <param name="YoutubeUrl">Chaîne YouTube, ou <see langword="null"/>.</param>
/// <param name="FacebookUrl">Page Facebook, ou <see langword="null"/>.</param>
/// <remarks>
/// Les champs sont nommés comme sur le site, en anglais : c'est la convention des contrats
/// d'API du projet. Une adresse laissée vide retire le réglage — et donc l'entrée du pied de
/// page. C'est ce qui permet de ne jamais afficher un lien mort.
/// </remarks>
public sealed record UpdateSiteSettingsRequest(
    [StringLength(500)] string? Logo = null,
    [StringLength(320)] string? Email = null,
    [StringLength(500)] string? WhatsappUrl = null,
    [StringLength(500)] string? GithubUrl = null,
    [StringLength(500)] string? LinkedinUrl = null,
    [StringLength(500)] string? XUrl = null,
    [StringLength(500)] string? YoutubeUrl = null,
    [StringLength(500)] string? FacebookUrl = null);