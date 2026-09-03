using System.ComponentModel.DataAnnotations;

namespace DotnetNiger.Api.Application.DTOs.Requests;

/// <summary>Requête d'abonnement à la newsletter.</summary>
public record SubscribeRequest(
    [Required(ErrorMessage = "L'email est requis.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    [StringLength(200, ErrorMessage = "L'email ne doit pas dépasser 200 caractères.")]
    string Email,
    [StringLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères.")]
    string? Name);

/// <summary>Requête de désabonnement de la newsletter.</summary>
public record UnsubscribeRequest(
    [Required(ErrorMessage = "L'email est requis.")]
    [EmailAddress(ErrorMessage = "Format d'email invalide.")]
    [StringLength(200, ErrorMessage = "L'email ne doit pas dépasser 200 caractères.")]
    string Email,
    [Required(ErrorMessage = "Le token est requis.")]
    string Token);

/// <summary>Requête de confirmation d'inscription à la newsletter.</summary>
public record ConfirmSubscriptionRequest(
    [Required(ErrorMessage = "Le token est requis.")]
    string Token);

/// <summary>Requête d'envoi d'une newsletter (blast) aux abonnés.</summary>
public record SendNewsletterRequest(
    [Required(ErrorMessage = "L'objet est requis.")]
    [StringLength(150, ErrorMessage = "L'objet ne doit pas dépasser 150 caractères.")]
    string Subject,
    [Required(ErrorMessage = "Le contenu est requis.")]
    string Content);
