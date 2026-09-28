using System.ComponentModel.DataAnnotations;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Application.Common;
using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Administration;

/// <summary>
/// Actions de l'administration, portées par les écrans du back-office.
/// </summary>
/// <remarks>
/// L'API JSON expose les mêmes opérations à <c>/api/admin/...</c>. Ces routes en sont la
/// contrepartie HTML : un écran doit pouvoir agir par un formulaire, sans JavaScript, et
/// reste soumis à la protection contre la falsification de requête — dont l'exclusion
/// s'applique aux seuls appels sous <c>/api</c>.
/// </remarks>
public static class AdministrationEndpoints
{
    /// <summary>
    /// Déclare les routes d'administration.
    /// </summary>
    /// <param name="app">Application en construction.</param>
    /// <returns>L'application, pour permettre le chaînage.</returns>
    public static WebApplication MapAdministrationEndpoints(this WebApplication app)
    {
        var administration = app.MapGroup("/administration").RequireAuthorization(policy =>
        {
            policy.RequireRole(RoleNames.Admin);
        });

        // ---------- Boîte de réception ----------

        administration.MapPost("/messages/{id:guid}/traiter", async (
            Guid id,
            IUnitOfWork unitOfWork,
            TimeProvider clock,
            CancellationToken cancellationToken) =>
        {
            var message = await unitOfWork.ReadRepository<ContactMessage>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

            if (message is null)
            {
                return Results.Redirect("/administration/messages?erreur=message-introuvable");
            }

            message.MarkHandled(clock.GetUtcNow().UtcDateTime);
            unitOfWork.Repository<ContactMessage>().Update(message);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Redirect("/administration/messages?traite=1");
        });

        administration.MapPost("/messages/{id:guid}/retirer", async (
            Guid id,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            TimeProvider clock,
            CancellationToken cancellationToken) =>
        {
            var message = await unitOfWork.ReadRepository<ContactMessage>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

            if (message is null)
            {
                return Results.Redirect("/administration/messages?erreur=message-introuvable");
            }

            unitOfWork.SoftDeletableRepository<ContactMessage>().Retire(
                message,
                clock.GetUtcNow().UtcDateTime,
                currentUser.UserId ?? throw new InvalidOperationException(
                    "Un administrateur authentifié a toujours un identifiant."));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Redirect("/administration/messages?retire=1");
        });

        // ---------- Actualités ----------

        administration.MapPost("/actualites/enregistrer", async (
            [FromForm] ActualiteInput input,
            Guid? id,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            INewsletterDiffusion newsletter,
            TimeProvider clock,
            CancellationToken cancellationToken) =>
        {
            if (ModelStateErreur(input, out var message))
            {
                return Results.Redirect(
                    $"/administration/actualites/{(id is { } cible ? cible.ToString() : "nouvelle")}?erreur={Uri.EscapeDataString(message)}");
            }

            var auteur = currentUser.UserId
                ?? throw new InvalidOperationException("Un administrateur authentifié a toujours un identifiant.");

            if (id is { } identifiant)
            {
                var article = await unitOfWork.ReadRepository<Article>().Query
                    .FirstOrDefaultAsync(candidat => candidat.Id == identifiant, cancellationToken);

                if (article is null)
                {
                    return Results.Redirect("/administration/actualites?erreur=actualite-introuvable");
                }

                // Réduire un article publié à un brouillon le ferait disparaître du site
                // sans prevenir : l'administration ne s'en sert pas, elle le republie.
                if (article.Status == PublicationStatus.Published)
                {
                    return Results.Redirect($"/administration/actualites/{identifiant}?erreur=deja-publie");
                }

                article.UpdateDetails(input.Title, input.Excerpt, input.Content, input.Category);
                article.SetCoverImage(string.IsNullOrWhiteSpace(input.CoverImageUrl) ? null : input.CoverImageUrl);

                var aRepublier = input.Publish;

                if (aRepublier)
                {
                    article.Publish(clock.GetUtcNow().UtcDateTime);
                }

                unitOfWork.Repository<Article>().Update(article);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                if (aRepublier)
                {
                    await newsletter.DiffuserAsync(NouvellePublication.De(article), cancellationToken);
                }

                return Results.Redirect($"/administration/actualites?publie={(aRepublier ? 1 : 0)}");
            }

            var nouvelArticle = new Article(auteur, input.Title, input.Excerpt, input.Content, input.Category);

            if (!string.IsNullOrWhiteSpace(input.CoverImageUrl))
            {
                nouvelArticle.SetCoverImage(input.CoverImageUrl);
            }

            var publie = input.Publish;

            if (publie)
            {
                nouvelArticle.Publish(clock.GetUtcNow().UtcDateTime);
            }

            unitOfWork.Repository<Article>().Add(nouvelArticle);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (publie)
            {
                await newsletter.DiffuserAsync(NouvellePublication.De(nouvelArticle), cancellationToken);
            }

            return Results.Redirect($"/administration/actualites?publie={(publie ? 1 : 0)}");
        });

        administration.MapPost("/actualites/{id:guid}/publier", async (
            Guid id,
            IUnitOfWork unitOfWork,
            INewsletterDiffusion newsletter,
            TimeProvider clock,
            CancellationToken cancellationToken) =>
        {
            var article = await unitOfWork.ReadRepository<Article>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

            if (article is null)
            {
                return Results.Redirect("/administration/actualites?erreur=actualite-introuvable");
            }

            // Republicer un article déjà publié enverrait la lettre une seconde fois aux
            // mêmes abonnés : une simple relecture deviendrait une deuxième publication.
            if (article.Status == PublicationStatus.Published)
            {
                return Results.Redirect("/administration/actualites?erreur=deja-publie");
            }

            article.Publish(clock.GetUtcNow().UtcDateTime);
            unitOfWork.Repository<Article>().Update(article);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await newsletter.DiffuserAsync(NouvellePublication.De(article), cancellationToken);

            return Results.Redirect("/administration/actualites?publie=1");
        });

        administration.MapPost("/actualites/{id:guid}/archiver", async (
            Guid id,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var article = await unitOfWork.ReadRepository<Article>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

            if (article is null)
            {
                return Results.Redirect("/administration/actualites?erreur=actualite-introuvable");
            }

            article.Archive();
            unitOfWork.Repository<Article>().Update(article);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Redirect("/administration/actualites?archive=1");
        });

        administration.MapPost("/actualites/{id:guid}/brouillon", async (
            Guid id,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var article = await unitOfWork.ReadRepository<Article>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

            if (article is null)
            {
                return Results.Redirect("/administration/actualites?erreur=actualite-introuvable");
            }

            article.ReturnToDraft();
            unitOfWork.Repository<Article>().Update(article);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Redirect("/administration/actualites?brouillon=1");
        });

        administration.MapPost("/actualites/{id:guid}/retirer", async (
            Guid id,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            TimeProvider clock,
            CancellationToken cancellationToken) =>
        {
            var article = await unitOfWork.ReadRepository<Article>().Query
                .FirstOrDefaultAsync(candidat => candidat.Id == id, cancellationToken);

            if (article is null)
            {
                return Results.Redirect("/administration/actualites?erreur=actualite-introuvable");
            }

            unitOfWork.SoftDeletableRepository<Article>().Retire(
                article,
                clock.GetUtcNow().UtcDateTime,
                currentUser.UserId ?? throw new InvalidOperationException(
                    "Un administrateur authentifié a toujours un identifiant."));

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Redirect("/administration/actualites?retire=1");
        });

        return app;
    }

    /// <summary>
    /// Traduit les annotations de validation en une phrase, la première suffice.
    /// </summary>
    private static bool ModelStateErreur(ActualiteInput input, out string message)
    {
        var echecs = new List<ValidationResult>();

        if (Validator.TryValidateObject(input, new ValidationContext(input), echecs, validateAllProperties: true)
            && echecs.Count == 0)
        {
            message = string.Empty;
            return false;
        }

        message = echecs.Count > 0 ? echecs[0].ErrorMessage! : "Le formulaire est incomplet.";
        return true;
    }
}
