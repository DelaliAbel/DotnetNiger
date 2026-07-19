namespace DotnetNiger.Community.Application.DTOs.Requests;

/// <summary>Requête d'abonnement à la newsletter.</summary>
public record SubscribeRequest(string Email, string Name);

/// <summary>Requête de désabonnement de la newsletter.</summary>
public record UnsubscribeRequest(string Email, string Token);

