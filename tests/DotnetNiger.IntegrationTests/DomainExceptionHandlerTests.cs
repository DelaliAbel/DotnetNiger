using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Web.Controller.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie la traduction des exceptions en réponses HTTP.
/// </summary>
/// <remarks>
/// Un jeton antiforgerie est validé avant que la requête n'atteigne le point de terminaison.
/// Laissant l'échec remonter, la page d'erreur du site répondait 500 : une session simplement
/// expirée se confondait avec une panne, et chaque formulaire abandonné noierait le suivi des
/// incidents. Ces tests verrouillent le code 400 et le refus de fuiter le détail technique.
/// </remarks>
public class DomainExceptionHandlerTests
{
    [Fact]
    public async Task Une_requete_mal_formee_repond_400_et_non_500()
    {
        var (traitee, reponse, _) = await TraiterAsync(new BadHttpRequestException(
            "The required antiforgery cookie is not present."));

        Assert.True(traitee);
        Assert.Equal(StatusCodes.Status400BadRequest, reponse.StatusCode);
    }

    [Fact]
    public async Task Une_requete_mal_formee_ne_divulgue_pas_le_detail_technique()
    {
        var (_, _, problemes) = await TraiterAsync(new BadHttpRequestException(
            "The required antiforgery cookie .AspNetCore.Antiforgery.zx6ha is not present."));

        // Le nom du cookie anti-CSRF dans une réponse publique n'aide que l'attaquant à
        // comprendre ce qui bloque sa requête ; le membre n'a besoin que du message à l'écran.
        Assert.DoesNotContain("Antiforgery", problemes.Detail ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Rechargez la page", problemes.Detail ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Une_regle_metier_refusee_garde_son_code_metier()
    {
        var (traitee, reponse, problemes) = await TraiterAsync(new DomainException(
            "connexion_refusee",
            "Adresse e-mail ou mot de passe incorrect."));

        Assert.True(traitee);
        Assert.Equal(StatusCodes.Status400BadRequest, reponse.StatusCode);
        Assert.EndsWith("connexion_refusee", problemes.Type ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Une_panne_inconnue_laisse_la_main_au_handler_suivant()
    {
        // Une panne n'est pas une requête refusée : la traduire en 400 masquerait le bug
        // derrière une réponse de formulaire, et le gestionnaire suivant ne verrait rien.
        var (traitee, _, problemes) = await TraiterAsync(new InvalidOperationException("Circuit ouvert."));

        Assert.False(traitee);
        Assert.Null(problemes.Title);
    }

    /// <summary>
    /// Exécute le gestionnaire sur une requête vide et relève ce qu'il a produit.
    /// </summary>
    /// <param name="exception">Exception à traduire.</param>
    /// <returns>Indicateur de traitement, réponse et détails du problème.</returns>
    private static async Task<(bool Traitee, HttpResponse Reponse, ProblemDetails Problemes)> TraiterAsync(
        Exception exception)
    {
        var contexte = new DefaultHttpContext();
        var capture = new ProblemDetailsCapturant();
        var handler = new DomainExceptionHandler(capture, NullLogger<DomainExceptionHandler>.Instance);

        var traitee = await handler.TryHandleAsync(contexte, exception, CancellationToken.None);

        return (traitee, contexte.Response, capture.Problemes);
    }

    /// <summary>
    /// Service de détails de problème qui mémorise l'écriture au lieu de l'écrire.
    /// </summary>
    private sealed class ProblemDetailsCapturant : IProblemDetailsService
    {
        /// <summary>Détails du problème écrit par le gestionnaire.</summary>
        public ProblemDetails Problemes { get; private set; } = new();

        /// <inheritdoc />
        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Problemes = context.ProblemDetails;

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Problemes = context.ProblemDetails;

            // Le service réel renvoie faux quand l'écriture a été refusé — par exemple
            // parce que la réponse a déjà commencé. Ici l'écriture aboutit toujours.
            return new ValueTask<bool>(true);
        }
    }
}
