using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Infrastructure.Content;

/// <summary>
/// Dépôt d'une ressource partagée par un membre.
/// </summary>
/// <remarks>
/// Le dépôt est la seule opération de la communauté sur les ressources : la même règle
/// sert l'API publique et l'écran de l'espace membre. La keeping ici évite que les deux
/// Points d'entrée divergent sur ce qui est publié, et sur ce qui déclenche la
/// lettre d'information.
/// </remarks>
public sealed class DepotRessources(
    IUnitOfWork unitOfWork,
    INewsletterDiffusion newsletter,
    TimeProvider clock)
{
    /// <summary>
    /// Dépose une ressource et la publie immédiatement.
    /// </summary>
    /// <remarks>
    /// Un membre dépose, il ne rédige pas de brouillon : la ressource est visible tout
    /// de suite et l'auteur peut encore la retirer. C'est le comportement historique de
    /// l'API, on ne le change pas ici.
    /// </remarks>
    /// <param name="auteurId">Identifiant du membre qui dépose.</param>
    /// <param name="titre">Titre de la ressource.</param>
    /// <param name="description">Description courte.</param>
    /// <param name="categorie">Catégorie de la ressource.</param>
    /// <param name="lienExterne">Adresse de la ressource, si elle vit ailleurs.</param>
    /// <param name="fournisseur">Nom de l'éditeur ou du site qui héberge la ressource.</param>
    /// <param name="imageCouverture">Adresse de l'illustration, si elle existe.</param>
    /// <param name="contenu">Contenu rédigé sur le site, si la ressource n'est pas un simple lien.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La ressource déposée, publiée.</returns>
    public async Task<Resource> DeposerAsync(
        Guid auteurId,
        string titre,
        string description,
        ResourceCategory categorie,
        string? lienExterne,
        string? fournisseur,
        string? imageCouverture,
        string? contenu = null,
        CancellationToken cancellationToken = default)
    {
        var ressource = new Resource(auteurId, titre, description, categorie);

        if (!string.IsNullOrWhiteSpace(contenu))
        {
            ressource.SetContent(contenu);
        }

        if (!string.IsNullOrWhiteSpace(lienExterne))
        {
            ressource.LinkTo(lienExterne, fournisseur);
        }

        if (!string.IsNullOrWhiteSpace(imageCouverture))
        {
            ressource.SetCoverImage(imageCouverture);
        }

        ressource.Publish(clock.GetUtcNow().UtcDateTime);

        unitOfWork.Repository<Resource>().Add(ressource);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await newsletter.DiffuserAsync(NouvellePublication.De(ressource), cancellationToken);

        return ressource;
    }

    /// <summary>
    /// Corrige une ressource déposée par un membre.
    /// </summary>
    /// <remarks>
    /// Le membre peut donc reprendre ce qu'il a publié : une faute dans le titre, une
    /// adresse changée, une catégorie mal choisie. La ressource reste en ligne pendant la
    /// correction, et la lettre d'information n'est pas rediffusée — le dépôt, lui, annonce
    /// la publication ; une correction, non.
    /// </remarks>
    /// <param name="auteurId">Identifiant du membre, auteur de la ressource.</param>
    /// <param name="ressourceId">Identifiant de la ressource à corriger.</param>
    /// <param name="titre">Nouveau titre.</param>
    /// <param name="description">Nouvelle description.</param>
    /// <param name="categorie">Nouvelle catégorie.</param>
    /// <param name="lienExterne">Adresse de la ressource, facultative.</param>
    /// <param name="fournisseur">Éditeur ou site, facultatif.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    public async Task CorrigerAsync(
        Guid auteurId,
        Guid ressourceId,
        string titre,
        string description,
        ResourceCategory categorie,
        string? lienExterne,
        string? fournisseur,
        CancellationToken cancellationToken = default)
    {
        var ressources = unitOfWork.Repository<Resource>();

        var ressource = await ressources.TrackedQuery
            .FirstOrDefaultAsync(entree => entree.Id == ressourceId, cancellationToken)
            ?? throw new DomainException("ressource-introuvable", "Cette ressource n'existe pas.");

        // Un membre ne modifie que ce qu'il a déposé : sans cette garde, l'identifiant
        // deviné dans le formulaire donnerait la main sur les ressources des autres.
        if (ressource.CreatorId != auteurId)
        {
            throw new DomainException("ressource-pas-a-vous", "Cette ressource n'est pas la vôtre.");
        }

        ressource.UpdateDetails(titre, description, categorie);

        if (!string.IsNullOrWhiteSpace(lienExterne))
        {
            ressource.LinkTo(lienExterne, fournisseur);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Retire définitivement une ressource déposée par un membre.
    /// </summary>
    /// <remarks>
    /// La suppression est définitive, comme celle que l'administration pratiquait. Un
    /// membre peut donc retirer ce qu'il a publié par erreur — ce qui est précisément le
    /// moment où l'on veut cette possibilité.
    /// </remarks>
    /// <param name="auteurId">Identifiant du membre, auteur de la ressource.</param>
    /// <param name="ressourceId">Identifiant de la ressource à retirer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    public async Task RetirerAsync(
        Guid auteurId,
        Guid ressourceId,
        CancellationToken cancellationToken = default)
    {
        var ressources = unitOfWork.Repository<Resource>();

        var ressource = await ressources.TrackedQuery
            .FirstOrDefaultAsync(entree => entree.Id == ressourceId, cancellationToken)
            ?? throw new DomainException("ressource-introuvable", "Cette ressource n'existe pas.");

        if (ressource.CreatorId != auteurId)
        {
            throw new DomainException("ressource-pas-a-vous", "Cette ressource n'est pas la vôtre.");
        }

        ressources.Remove(ressource);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}