using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Administration;

/// <summary>Un message reçu par le formulaire de contact.</summary>
/// <param name="Id">Identifiant.</param>
/// <param name="Nom">Nom de l'expéditeur.</param>
/// <param name="Courriel">Adresse de réponse.</param>
/// <param name="Sujet">Objet du message.</param>
/// <param name="Corps">Texte du message.</param>
/// <param name="RecuLe">Date de réception (UTC).</param>
/// <param name="TraiteLe">Date de traitement (UTC), ou <see langword="null"/> si sans réponse.</param>
public sealed record LigneMessageAdmin(
    Guid Id,
    string Nom,
    string Courriel,
    string Sujet,
    string Corps,
    DateTime RecuLe,
    DateTime? TraiteLe);

/// <summary>Tous les messages, lus pour la page Messages.</summary>
/// <param name="Messages">Messages, du plus récent au plus ancien.</param>
public sealed record MessagesVue(IReadOnlyList<LigneMessageAdmin> Messages);

/// <summary>
/// Lecture de la boîte de réception pour l'administration. Les écritures passent par les
/// points d'extrémité de l'API.
/// </summary>
/// <remarks>
/// La liste ne filtre aucun état : un message ayant reçu une réponse doit rester visible,
/// comme on relit ce qu'on a répondu. C'est l'écran qui décide de n'afficher que ceux qui
/// attendent une réponse.
/// </remarks>
public sealed class ServiceMessages(IUnitOfWork uniteDeTravail)
{
    private Task<MessagesVue>? _lecture;

    /// <summary>Lit la boîte de réception, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les messages reçus.</returns>
    public Task<MessagesVue> LireAsync(CancellationToken cancellationToken = default)
        => (_lecture ??= LireInterneAsync(cancellationToken));

    /// <summary>Relit la liste à la prochaine demande, après une écriture.</summary>
    public void Oublie() => _lecture = null;

    private async Task<MessagesVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        var messages = await uniteDeTravail.ReadRepository<ContactMessage>().Query
            .AsNoTracking()
            .OrderByDescending(message => message.ReceivedAtUtc)
            .ToListAsync(cancellationToken);

        return new MessagesVue(
            [.. messages.Select(message => new LigneMessageAdmin(
                message.Id,
                message.Name,
                message.Email,
                message.Subject,
                message.Body,
                message.ReceivedAtUtc,
                message.HandledAtUtc))]);
    }
}