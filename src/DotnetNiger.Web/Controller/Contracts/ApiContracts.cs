using System.ComponentModel.DataAnnotations;
using DotnetNiger.Domain.Entities.Comments;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Entities.Partners;
using DotnetNiger.Domain.Entities.Projects;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Identity;

namespace DotnetNiger.Web.Controller.Contracts;

/// <summary>
/// Projets de l'API publique.
/// </summary>
public static class ApiRoutes
{
    /// <summary>
    /// Préfixe commun des routes. Aucun versionnement n'est appliqué : le contrat est en
    /// construction et n'a pas encore été publié à des clients qu'il serait coûteux de
    /// rompre.
    /// </summary>
    public const string Base = "/api";
}

/// <summary>
/// Paramètres de pagination acceptés par les listes.
/// </summary>
/// <param name="Page">Numéro de page, à partir de 1.</param>
/// <param name="PageSize">Nombre d'éléments par page.</param>
public readonly record struct PageQuery(int Page = 1, int PageSize = 20);

/// <summary>
/// Page de résultats exposée par l'API.
/// </summary>
/// <typeparam name="T">Type des éléments.</typeparam>
/// <param name="Items">Éléments de la page.</param>
/// <param name="Page">Numéro de page.</param>
/// <param name="PageSize">Taille de page.</param>
/// <param name="TotalCount">Nombre total d'éléments correspondant au filtre.</param>
/// <param name="TotalPages">Nombre total de pages.</param>
/// <param name="HasNext">Vrai s'il existe une page suivante.</param>
public sealed record PageResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasNext);

/// <summary>Résumé d'un article publié.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="PublishedAtUtc">Date de publication.</param>
public sealed record ArticleSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    ArticleCategory Category,
    string Author,
    DateTime? PublishedAtUtc);

/// <summary>Article complet.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Content">Contenu rédigé.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="CoverImageUrl">Image de couverture.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="PublishedAtUtc">Date de publication.</param>
public sealed record ArticleResponse(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    string Content,
    ArticleCategory Category,
    string? CoverImageUrl,
    string Author,
    DateTime? PublishedAtUtc);

/// <summary>Article tel que l'administration le voit : tous les états, pas seulement le publié.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Status">État de publication.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="PublishedAtUtc">Date de publication.</param>
/// <param name="UpdatedAtUtc">Dernière modification, ou <see langword="null"/> si l'article est resté intact depuis sa création.</param>
public sealed record AdminArticleSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    ArticleCategory Category,
    PublicationStatus Status,
    string Author,
    DateTime? PublishedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>Demande de création d'un article.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Content">Contenu rédigé.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Publish">Publier immédiatement plutôt que garder en brouillon.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette du blog.</param>
public sealed record CreateArticleRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Excerpt,
    [Required] string Content,
    ArticleCategory Category,
    bool Publish = false,
    [StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Résumé d'une ressource.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="ExternalUrl">URL externe, si la ressource est un lien.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record ResourceSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Description,
    ResourceCategory Category,
    string? Provider,
    string? ExternalUrl,
    string? CoverImageUrl);

/// <summary>Demande de création d'une ressource.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="ExternalUrl">URL de la ressource externe, si applicable.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record CreateResourceRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Description,
    ResourceCategory Category,
    [StringLength(2048)] string? ExternalUrl = null,
    [StringLength(120)] string? Provider = null,
    [StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Résumé d'un événement.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si limité.</param>
/// <param name="ConfirmedCount">Inscriptions confirmées.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record EventSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? OnlineUrl,
    int? Capacity,
    int ConfirmedCount,
    string? CoverImageUrl);

/// <summary>Demande d'inscription à un événement.</summary>
/// <param name="EventId">Identifiant de l'événement.</param>
public sealed record RegisterForEventRequest(
    [Required] Guid EventId);

/// <summary>Résumé d'un projet.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="ImageUrl">Image de présentation, pour la vignette.</param>
public sealed record ProjectSummaryResponse(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string? Technologies,
    string RepositoryUrl,
    string? ImageUrl);

/// <summary>Demande de création d'un projet.</summary>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="ImageUrl">Image de présentation, pour la vignette.</param>
public sealed record CreateProjectRequest(
    [Required, StringLength(160)] string Name,
    [Required, StringLength(500)] string Summary,
    [Required] string Description,
    [Required, StringLength(2048)] string RepositoryUrl,
    [StringLength(500)] string? Technologies = null,
    [StringLength(2048)] string? ImageUrl = null);

/// <summary>Commentaire publié.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="ArticleId">Article commenté, si applicable.</param>
/// <param name="CommunityEventId">Événement commenté, si applicable.</param>
/// <param name="ParentCommentId">Commentaire parent, pour une réponse.</param>
/// <param name="Body">Texte du commentaire.</param>
/// <param name="Author">Nom de l'auteur.</param>
/// <param name="CreatedAtUtc">Date de publication.</param>
public sealed record CommentResponse(
    Guid Id,
    Guid? ArticleId,
    Guid? CommunityEventId,
    Guid? ParentCommentId,
    string Body,
    string Author,
    DateTime CreatedAtUtc);

/// <summary>Demande de publication d'un commentaire.</summary>
/// <param name="Body">Texte du commentaire.</param>
/// <param name="ArticleId">Article commenté, si applicable.</param>
/// <param name="EventId">Événement commenté, si applicable.</param>
/// <param name="ParentCommentId">Commentaire parent, pour une réponse.</param>
public sealed record CreateCommentRequest(
    [Required, StringLength(4000)] string Body,
    Guid? ArticleId = null,
    Guid? EventId = null,
    Guid? ParentCommentId = null);

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

/// <summary>Partenaire affiché sur le site.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Name">Nom.</param>
/// <param name="Description">Description.</param>
/// <param name="WebsiteUrl">Site web.</param>
/// <param name="LogoUrl">Logo.</param>
/// <param name="Tier">Niveau de partenariat.</param>
public sealed record PartnerResponse(
    Guid Id,
    string Name,
    string Description,
    string WebsiteUrl,
    string? LogoUrl,
    PartnerTier Tier);

/// <summary>Signalement de contenu.</summary>
/// <param name="Reason">Motif du signalement.</param>
/// <param name="Details">Précisions facultatives.</param>
public sealed record CreateReportRequest(
    ReportReason Reason,
    [StringLength(1000)] string? Details = null);

/// <summary>État d'un signalement, réservé à la modération.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="TargetType">Type de cible.</param>
/// <param name="TargetId">Identifiant de la cible.</param>
/// <param name="Reason">Motif.</param>
/// <param name="Status">État de traitement.</param>
/// <param name="CreatedAtUtc">Date de création.</param>
public sealed record ReportResponse(
    Guid Id,
    ReportTargetType TargetType,
    Guid TargetId,
    ReportReason Reason,
    ReportStatus Status,
    DateTime CreatedAtUtc);

/// <summary>Demande de mise à jour d'un article.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Excerpt">Chapô.</param>
/// <param name="Content">Contenu rédigé.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="CoverImageUrl">Image de couverture, ou <see langword="null"/> pour la retirer.</param>
public sealed record UpdateArticleRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Excerpt,
    [Required] string Content,
    ArticleCategory Category,
    [StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Demande de mise à jour d'une ressource.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Description">Description courte.</param>
/// <param name="Category">Catégorie.</param>
/// <param name="Content">Contenu rédigé, si la ressource est hébergée sur le site.</param>
/// <param name="ExternalUrl">URL de la ressource externe.</param>
/// <param name="Provider">Éditeur ou source.</param>
/// <param name="CoverImageUrl">Image de couverture, ou <see langword="null"/> pour la retirer.</param>
public sealed record UpdateResourceRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Description,
    ResourceCategory Category,
    [StringLength(32000)] string? Content = null,
    [StringLength(2048)] string? ExternalUrl = null,
    [StringLength(120)] string? Provider = null,
    [StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Demande de mise à jour d'un projet.</summary>
/// <param name="Name">Nom du projet.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="RepositoryUrl">Dépôt.</param>
/// <param name="DemoUrl">Démonstration en ligne.</param>
/// <param name="Technologies">Technologies utilisées.</param>
/// <param name="ImageUrl">Image de présentation, ou <see langword="null"/> pour la retirer.</param>
public sealed record UpdateProjectRequest(
    [Required, StringLength(160)] string Name,
    [Required, StringLength(500)] string Summary,
    [Required] string Description,
    [Required, StringLength(2048)] string RepositoryUrl,
    [StringLength(2048)] string? DemoUrl = null,
    [StringLength(500)] string? Technologies = null,
    [StringLength(2048)] string? ImageUrl = null);

/// <summary>Demande de création d'un événement.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si l'événement est limité.</param>
/// <param name="RegistrationClosesAtUtc">Fin des inscriptions.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
/// <param name="Publish">Publier immédiatement plutôt que garder en brouillon.</param>
public sealed record CreateEventRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Summary,
    [Required, StringLength(32000)] string Description,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    [StringLength(150)] string? Location = null,
    [StringLength(2048)] string? OnlineUrl = null,
    int? Capacity = null,
    DateTime? RegistrationClosesAtUtc = null,
    [StringLength(2048)] string? CoverImageUrl = null,
    bool Publish = false);

/// <summary>Demande de mise à jour d'un événement.</summary>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si l'événement est limité.</param>
/// <param name="RegistrationClosesAtUtc">Fin des inscriptions.</param>
/// <param name="CoverImageUrl">Image de couverture, ou <see langword="null"/> pour la retirer.</param>
public sealed record UpdateEventRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(500)] string Summary,
    [Required, StringLength(32000)] string Description,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    [StringLength(150)] string? Location = null,
    [StringLength(2048)] string? OnlineUrl = null,
    int? Capacity = null,
    DateTime? RegistrationClosesAtUtc = null,
    [StringLength(2048)] string? CoverImageUrl = null);

/// <summary>Événement complet.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Slug">Slug d'URL.</param>
/// <param name="Title">Titre.</param>
/// <param name="Summary">Résumé.</param>
/// <param name="Description">Description détaillée.</param>
/// <param name="Format">Présentiel, en ligne ou hybride.</param>
/// <param name="StartsAtUtc">Début.</param>
/// <param name="EndsAtUtc">Fin.</param>
/// <param name="Location">Lieu, pour un événement en présentiel.</param>
/// <param name="OnlineUrl">Lien de participation, en ligne.</param>
/// <param name="Capacity">Nombre de places, si l'événement est limité.</param>
/// <param name="RegistrationClosesAtUtc">Fin des inscriptions.</param>
/// <param name="Status">État de publication.</param>
/// <param name="ConfirmedCount">Inscriptions confirmées.</param>
/// <param name="CoverImageUrl">Image de couverture, pour la vignette.</param>
public sealed record EventResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string Description,
    CommunityEventFormat Format,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string? Location,
    string? OnlineUrl,
    int? Capacity,
    DateTime? RegistrationClosesAtUtc,
    PublicationStatus Status,
    int ConfirmedCount,
    string? CoverImageUrl);

/// <summary>Inscription à un événement.</summary>
/// <param name="Id">Identifiant de l'inscription.</param>
/// <param name="MemberId">Membre inscrit.</param>
/// <param name="MemberName">Nom du membre inscrit.</param>
/// <param name="Status">État de l'inscription.</param>
/// <param name="RegisteredAtUtc">Date d'inscription.</param>
/// <param name="CancelledAtUtc">Date d'annulation, si annulée.</param>
/// <param name="CheckedInAtUtc">Date de présence, si l'inscription a été honorée.</param>
public sealed record EventRegistrationResponse(
    Guid Id,
    Guid MemberId,
    string MemberName,
    EventRegistrationStatus Status,
    DateTime RegisteredAtUtc,
    DateTime? CancelledAtUtc,
    DateTime? CheckedInAtUtc);

/// <summary>Demande de création d'un partenaire.</summary>
/// <param name="Name">Nom.</param>
/// <param name="Description">Description.</param>
/// <param name="WebsiteUrl">Site web.</param>
/// <param name="LogoUrl">Logo.</param>
/// <param name="Tier">Niveau de partenariat.</param>
/// <param name="SortOrder">Ordre d'affichage.</param>
public sealed record CreatePartnerRequest(
    [Required, StringLength(150)] string Name,
    [Required, StringLength(500)] string Description,
    [Required, StringLength(2048)] string WebsiteUrl,
    PartnerTier Tier,
    [StringLength(2048)] string? LogoUrl = null,
    int SortOrder = 0);

/// <summary>Demande de mise à jour d'un partenaire.</summary>
/// <param name="Name">Nom.</param>
/// <param name="Description">Description.</param>
/// <param name="WebsiteUrl">Site web.</param>
/// <param name="LogoUrl">Logo, ou <see langword="null"/> pour le retirer.</param>
/// <param name="Tier">Niveau de partenariat.</param>
/// <param name="SortOrder">Ordre d'affichage.</param>
public sealed record UpdatePartnerRequest(
    [Required, StringLength(150)] string Name,
    [Required, StringLength(500)] string Description,
    [Required, StringLength(2048)] string WebsiteUrl,
    PartnerTier Tier,
    [StringLength(2048)] string? LogoUrl = null,
    int SortOrder = 0);

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

/// <summary>Compte tel que l'administration le consulte.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Email">Adresse électronique.</param>
/// <param name="Slug">Slug du profil.</param>
/// <param name="FullName">Nom complet.</param>
/// <param name="IsActive">Compte actif, ou désactivé par l'administration.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
/// <param name="EmailConfirmed">Adresse électronique confirmée.</param>
/// <param name="Roles">Rôles attribués.</param>
/// <param name="LockoutEnd">Fin de verrouillage, après trop de tentatives.</param>
/// <param name="CreatedAtUtc">Date d'inscription.</param>
public sealed record AdminAccountResponse(
    Guid Id,
    string? Email,
    string? Slug,
    string FullName,
    bool IsActive,
    bool IsTeam,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles,
    DateTimeOffset? LockoutEnd,
    DateTime CreatedAtUtc);

/// <summary>Demande de modération d'un compte.</summary>
/// <param name="IsActive">Vrai pour réactiver, faux pour désactiver.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
public sealed record UpdateAccountRequest(
    bool IsActive,
    bool IsTeam);

/// <summary>Rôles à attribuer à un compte.</summary>
/// <param name="Roles">Rôles complets attendus après l'opération.</param>
public sealed record ChangeRolesRequest(
    [Required, MinLength(1)] IReadOnlyList<string> Roles);

/// <summary>Création d'un compte par l'administration.</summary>
/// <param name="Email">Adresse de connexion.</param>
/// <param name="Password">Mot de passe initial, soumis à la politique du site.</param>
/// <param name="FirstName">Prénom, facultatif.</param>
/// <param name="LastName">Nom, facultatif.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
/// <param name="ConfirmEmail">
/// Vrai si l'administration atteste que l'adresse a été contrôlée par un autre canal.
/// À faux, le membre devra confirmer son adresse avant de se connecter.
/// </param>
/// <param name="Roles">Rôles attribués. <c>Member</c> si la liste est omise.</param>
public sealed record CreateAccountRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password,
    [StringLength(100)] string? FirstName = null,
    [StringLength(100)] string? LastName = null,
    bool IsTeam = false,
    bool ConfirmEmail = false,
    IReadOnlyList<string>? Roles = null);

/// <summary>Suppression définitive du compte du membre connecté.</summary>
/// <param name="ConfirmEmail">Adresse du compte, à recopier pour confirmer.</param>
/// <param name="ConfirmPermanentDeletion">Vrai pour confirmer l'irréversibilité.</param>
/// <remarks>
/// Deux confirmations sont exigées parce que l'opération est irréversible : l'interface
/// les présente l'une après l'autre, et le serveur refuse une demande qui n'en porte
/// qu'une, afin qu'un appel isolé — un lien suivi par erreur, un script — ne puisse pas
/// effacer un compte.
/// </remarks>
public sealed record DeleteOwnAccountRequest(
    [Required, EmailAddress, StringLength(256)] string ConfirmEmail,
    bool ConfirmPermanentDeletion = false);

/// <summary>Projets internes, pour la construction des réponses.</summary>
internal static class ApiProjections
{
    /// <summary>
    /// Projette un article en résumé.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ArticleSummaryResponse ToSummary(Article article, string auteur) =>
        new(article.Id, article.Slug, article.Title, article.Excerpt, article.Category, auteur, article.PublishedAtUtc);

    /// <summary>
    /// Projette un article en résumé d'administration, état de publication compris.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static AdminArticleSummaryResponse ToAdminSummary(Article article, string auteur) =>
        new(
            article.Id,
            article.Slug,
            article.Title,
            article.Excerpt,
            article.Category,
            article.Status,
            auteur,
            article.PublishedAtUtc,
            article.UpdatedAtUtc);

    /// <summary>
    /// Projette un article en réponse complète.
    /// </summary>
    /// <param name="article">Article à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static ArticleResponse ToDetail(Article article, string auteur) =>
        new(
            article.Id,
            article.Slug,
            article.Title,
            article.Excerpt,
            article.Content,
            article.Category,
            article.CoverImageUrl,
            auteur,
            article.PublishedAtUtc);

    /// <summary>
    /// Projette une ressource en résumé.
    /// </summary>
    /// <param name="resource">Ressource à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ResourceSummaryResponse ToSummary(Resource resource) =>
        new(
            resource.Id,
            resource.Slug,
            resource.Title,
            resource.Description,
            resource.Category,
            resource.Provider,
            resource.ExternalUrl,
            resource.CoverImageUrl);

    /// <summary>
    /// Projette un événement en résumé.
    /// </summary>
    /// <param name="communityEvent">Événement à projeter.</param>
    /// <param name="confirmes">Nombre d'inscriptions confirmées.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static EventSummaryResponse ToSummary(CommunityEvent communityEvent, int confirmes) =>
        new(
            communityEvent.Id,
            communityEvent.Slug,
            communityEvent.Title,
            communityEvent.Summary,
            communityEvent.Format,
            communityEvent.StartsAtUtc,
            communityEvent.EndsAtUtc,
            communityEvent.Location,
            communityEvent.OnlineUrl,
            communityEvent.Capacity,
            confirmes,
            communityEvent.CoverImageUrl);

    /// <summary>
    /// Projette un événement en réponse complète.
    /// </summary>
    /// <param name="communityEvent">Événement à projeter.</param>
    /// <param name="confirmes">Nombre d'inscriptions confirmées.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static EventResponse ToDetail(CommunityEvent communityEvent, int confirmes) =>
        new(
            communityEvent.Id,
            communityEvent.Slug,
            communityEvent.Title,
            communityEvent.Summary,
            communityEvent.Description,
            communityEvent.Format,
            communityEvent.StartsAtUtc,
            communityEvent.EndsAtUtc,
            communityEvent.Location,
            communityEvent.OnlineUrl,
            communityEvent.Capacity,
            communityEvent.RegistrationClosesAtUtc,
            communityEvent.Status,
            confirmes,
            communityEvent.CoverImageUrl);

    /// <summary>
    /// Projette une inscription à un événement.
    /// </summary>
    /// <param name="registration">Inscription à projeter.</param>
    /// <param name="membreNom">Nom du membre inscrit, résolu hors requête.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static EventRegistrationResponse ToResponse(EventRegistration registration, string membreNom) =>
        new(
            registration.Id,
            registration.MemberId,
            membreNom,
            registration.Status,
            registration.RegisteredAtUtc,
            registration.CancelledAtUtc,
            registration.CheckedInAtUtc);

    /// <summary>
    /// Projette un compte pour l'administration.
    /// </summary>
    /// <param name="user">Compte à projeter.</param>
    /// <param name="roles">Rôles attribués, résolus hors requête.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static AdminAccountResponse ToAccount(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(
            user.Id,
            user.Email,
            user.Slug,
            user.FullName,
            user.IsActive,
            user.IsTeam,
            user.EmailConfirmed,
            roles,
            user.LockoutEnd,
            user.CreatedAtUtc);

    /// <summary>
    /// Projette un projet en résumé.
    /// </summary>
    /// <param name="project">Projet à projeter.</param>
    /// <returns>Résumé sérialisable.</returns>
    public static ProjectSummaryResponse ToSummary(Project project) =>
        new(
            project.Id,
            project.Slug,
            project.Name,
            project.Summary,
            project.Technologies,
            project.RepositoryUrl,
            project.ImageUrl);

    /// <summary>
    /// Projette un commentaire publié.
    /// </summary>
    /// <param name="comment">Commentaire à projeter.</param>
    /// <param name="auteur">Nom de l'auteur, résolu hors requête.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static CommentResponse ToResponse(Comment comment, string auteur) =>
        new(
            comment.Id,
            comment.ArticleId,
            comment.CommunityEventId,
            comment.ParentCommentId,
            comment.Body,
            auteur,
            comment.CreatedAtUtc);

    /// <summary>
    /// Projette un profil public de membre.
    /// </summary>
    /// <param name="user">Membre à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static MemberResponse ToResponse(ApplicationUser user) =>
        new(
            user.Id,
            user.Slug,
            user.FullName,
            user.Bio,
            user.City,
            user.PhotoUrl,
            user.Technologies,
            user.IsTeam);

    /// <summary>
    /// Projette un partenaire.
    /// </summary>
    /// <param name="partner">Partenaire à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static PartnerResponse ToResponse(Partner partner) =>
        new(
            partner.Id,
            partner.Name,
            partner.Description,
            partner.WebsiteUrl,
            partner.LogoUrl,
            partner.Tier);

    /// <summary>
    /// Projette un signalement pour la modération.
    /// </summary>
    /// <param name="report">Signalement à projeter.</param>
    /// <returns>Réponse sérialisable.</returns>
    public static ReportResponse ToResponse(Report report) =>
        new(report.Id, report.TargetType, report.TargetId, report.Reason, report.Status, report.CreatedAtUtc);

    /// <summary>
    /// Construit une page de réponses à partir d'un résultat paginé du domaine.
    /// </summary>
    /// <typeparam name="TSource">Type des entités.</typeparam>
    /// <typeparam name="TResult">Type des réponses.</typeparam>
    /// <param name="result">Résultat paginé.</param>
    /// <param name="selector">Projection appliquée à chaque entité.</param>
    /// <returns>Page sérialisable.</returns>
    public static PageResponse<TResult> ToPage<TSource, TResult>(
        Application.Common.PagedResult<TSource> result,
        Func<TSource, TResult> selector) =>
        new(
            [.. result.Items.Select(selector)],
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasNext);
}

/// <summary>Accusé de mise en file d'un essai de modèle de courriel.</summary>
/// <param name="Cle">Clé du modèle essayé.</param>
/// <param name="Titre">Intitulé lisible du modèle.</param>
/// <param name="Destinataire">Adresse qui recevra le message, toujours celle du compte connecté.</param>
/// <param name="Objet">Objet du message mis en file.</param>
/// <param name="Texte">Version texte brut, pour relire l'envoi sans ouvrir une boîte mail.</param>
public sealed record EmailTestQueuedResponse(
    string Cle,
    string Titre,
    string Destinataire,
    string Objet,
    string Texte);

/// <summary>Message déposé via le formulaire de contact.</summary>
/// <param name="Name">Nom de l'auteur du message.</param>
/// <param name="Email">Adresse à laquelle répondre.</param>
/// <param name="Subject">Objet du message.</param>
/// <param name="Message">Corps du message.</param>
public sealed record CreateContactMessageRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(320)] string Email,
    [Required, StringLength(200, MinimumLength = 2)] string Subject,
    [Required, StringLength(32_000, MinimumLength = 10)] string Message);

/// <summary>Message de contact exposé à l'administration.</summary>
/// <param name="Id">Identifiant du message.</param>
/// <param name="Name">Nom de l'auteur du message.</param>
/// <param name="Email">Adresse de l'auteur.</param>
/// <param name="Subject">Objet du message.</param>
/// <param name="Message">Corps du message.</param>
/// <param name="ReceivedAtUtc">Instant de réception (UTC).</param>
/// <param name="HandledAtUtc">Instant de traitement (UTC), ou <see langword="null"/>.</param>
public sealed record ContactMessageResponse(
    Guid Id,
    string Name,
    string Email,
    string Subject,
    string Message,
    DateTime ReceivedAtUtc,
    DateTime? HandledAtUtc);

/// <summary>Demande d'abonnement à la lettre d'information.</summary>
/// <param name="Email">Adresse à abonner.</param>
public sealed record SubscribeToNewsletterRequest(
    [Required, EmailAddress, StringLength(320)] string Email);

/// <summary>Abonnement exposé à l'administration.</summary>
/// <param name="Id">Identifiant de l'abonnement.</param>
/// <param name="Email">Adresse abonnée.</param>
/// <param name="Status">État de l'abonnement.</param>
/// <param name="RequestedAtUtc">Instant de la demande (UTC).</param>
/// <param name="ConfirmedAtUtc">Instant de la confirmation (UTC), ou <see langword="null"/>.</param>
/// <param name="UnsubscribedAtUtc">Instant du désabonnement (UTC), ou <see langword="null"/>.</param>
public sealed record NewsletterSubscriptionResponse(
    Guid Id,
    string Email,
    NewsletterStatus Status,
    DateTime RequestedAtUtc,
    DateTime? ConfirmedAtUtc,
    DateTime? UnsubscribedAtUtc);


