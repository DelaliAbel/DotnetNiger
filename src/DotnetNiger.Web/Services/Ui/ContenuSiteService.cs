using DotnetNiger.Application.Abstractions;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Infrastructure.Queries;

namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Contenus du site public, lus dans la base par les mêmes services que l'API.
/// </summary>
/// <remarks>
/// <para>
/// Ce service remplace la garniture codée en dur. Les articles, événements, ressources,
/// projets et partenaires viennent désormais des tables, à travers les services de
/// lecture que les contrôleurs d'API utilisent eux aussi : le site et l'API ne peuvent
/// donc pas diverger, et un contenu retiré de la base disparaît des deux côtés.
/// </para>
/// <para>
/// Deux choses restent volontairement locales. La page À propos — mission, valeurs,
/// jalons de l'histoire — décrit l'identité de l'association : ces textes ne sont pas
/// des contenus éditorials à publier ou archiver, mais la parole du site sur lui-même.
/// Les canaux de la community page relèvent de la même logique : ils changent avec les
/// coordonnéesfficiales, pas avec le contenu publié. Les deux sont groupés dans
/// <see cref="TextesDuSite"/>, et non dispersés dans cette classe.
/// </para>
/// </remarks>
public sealed class ContenuSiteService(
    ArticleQueries articles,
    EvenementQueries evenements,
    CatalogueQueries catalogues,
    IMemberDirectory annuaire) : ISiteContentService
{
    /// <summary>Page d'accueil : les derniers contenus, lus en base.</summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les éléments de l'accueil.</returns>
    public async Task<AccueilVue> ChargerAccueilAsync(CancellationToken annulation = default)
    {
        var (articlesRecents, _) = await articles.ListerAsync(null, null, "recent", 1, 3, annulation);
        var (evenementsAVenir, _) = await evenements.ListerAsync(true, null, null, null, 1, 3, annulation);
        var (ressources, _) = await catalogues.ListerRessourcesAsync(null, null, null, 1, 3, annulation);
        var (_, nombreProjets) = await catalogues.ListerProjetsAsync(null, null, null, 1, 1, annulation);
        var (_, totalRessources) = await catalogues.ListerRessourcesAsync(null, null, null, 1, 1, annulation);
        var (_, totalEvenements) = await evenements.ListerAsync(null, null, null, null, 1, 1, annulation);
        var nombreMembres = await annuaire.CompterMembresAsync(annulation);

        return new AccueilVue
        {
            ArticlesRecents = [.. articlesRecents.Select(EnVue)],
            EvenementsAVenir = [.. evenementsAVenir.Select(EnVue)],
            RessourcesPopulaires = [.. ressources.Select(EnVue)],
            NombreMembres = nombreMembres,
            NombreProjets = nombreProjets,
            NombreRessources = totalRessources,
            NombreEvenements = totalEvenements,
        };
    }

    /// <summary>Tous les articles publiés.</summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les articles, du plus récent au plus ancien.</returns>
    public async Task<IReadOnlyList<ArticleVue>> ChargerArticlesAsync(CancellationToken annulation = default)
    {
        // Une seule page très large suffit : les pages du site filtrent et paginent
        // elles-mêmes, et fournir tous les articles par pages de l'API coûterait
        // un aller-retour par tranche de dix.
        var (resultats, _) = await articles.ListerAsync(null, null, "recent", 1, 500, annulation);
        return [.. resultats.Select(EnVue)];
    }

    /// <summary>Tous les événements publiés.</summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les événements, du plus ancien rendez-vous au plus lointain.</returns>
    public async Task<IReadOnlyList<EvenementVue>> ChargerEvenementsAsync(CancellationToken annulation = default)
    {
        var (resultats, _) = await evenements.ListerAsync(null, null, null, null, 1, 500, annulation);
        return [.. resultats.Select(EnVue)];
    }

    /// <summary>Toutes les ressources publiées.</summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les ressources, de la plus récente à la plus ancienne.</returns>
    public async Task<IReadOnlyList<RessourceVue>> ChargerRessourcesAsync(CancellationToken annulation = default)
    {
        var (resultats, _) = await catalogues.ListerRessourcesAsync(null, null, null, 1, 500, annulation);
        return [.. resultats.Select(EnVue)];
    }

    /// <summary>Tous les projets publiés.</summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les projets.</returns>
    public async Task<IReadOnlyList<ProjetVue>> ChargerProjetsAsync(CancellationToken annulation = default)
    {
        var (resultats, _) = await catalogues.ListerProjetsAsync(null, null, null, 1, 500, annulation);
        return [.. resultats.Select(EnVue)];
    }

    /// <summary>Partenaires et sponsors actifs.</summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les organisations, dans l'ordre d'affichage.</returns>
    public async Task<IReadOnlyList<PartenaireVue>> ChargerPartenairesAsync(CancellationToken annulation = default)
    {
        var partenaires = await catalogues.ListerPartenairesAsync(null, annulation);
        return [.. partenaires.Select(EnVue)];
    }

    /// <summary>
    /// L'équipe de la communauté, lue dans la table des comptes.
    /// </summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les membres de l'équipe.</returns>
    public async Task<IReadOnlyList<MembreVue>> ChargerMembresAsync(CancellationToken annulation = default)
    {
        var equipe = await annuaire.ListerEquipeAsync(annulation);

        return [.. equipe.Select(membre => new MembreVue
        {
            Nom = membre.FullName,
            Role = membre.Role ?? string.Empty,
            Teinte = TeinteDe(membre.FullName),
            // La photo vient du compte ; à défaut, la carte tombe sur les initiales
            // calculées par le composant. Aucune image de substitution n'est inventée :
            // une photo qui ne représenterait personne serait pire qu'un cercle vide.
            ImageUrl = membre.PhotoUrl,
            Liens = [.. membre.Liens.Select(lien => new LienEquipeVue(
                lien.Libelle,
                lien.Url,
                IconesDeLien.Pour(lien.Url)))],
        })];
    }

    /// <summary>Page À propos : textes du site, équipe tirée des auteurs réels.</summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les éléments de la page.</returns>
    public async Task<AProposVue> ChargerAProposAsync(CancellationToken annulation = default) => new()
    {
        Presentation = TextesDuSite.Presentation,
        Mission = TextesDuSite.Mission,
        MissionDetail = TextesDuSite.MissionDetail,
        Valeurs = TextesDuSite.Valeurs,
        Histoire = TextesDuSite.Jalons,
        Equipe = await ChargerMembresAsync(annulation),
    };

    /// <summary>Page communauté : événements à venir, projets et canaux.</summary>
    /// <param name="annulation">Jeton d'annulation.</param>
    /// <returns>Les éléments de la page.</returns>
    public async Task<CommunauteVue> ChargerCommunauteAsync(CancellationToken annulation = default)
    {
        // Seuls les rendez-vous à venir : la page communauté annonce ce qui va se passer,
        // et l'agenda complet, lui, garde aussi le passé.
        var (listeEvenements, totalEvenements) = await evenements.ListerAsync(true, null, null, null, 1, 200, annulation);
        var (projets, totalProjets) = await catalogues.ListerProjetsAsync(null, null, null, 1, 200, annulation);
        var nombreMembres = await annuaire.CompterMembresAsync(annulation);

        return new CommunauteVue
        {
            Evenements = [.. listeEvenements.Select(EnVue)],
            Projets = [.. projets.Select(EnVue)],
            NombreMembres = nombreMembres,
            NombreProjets = totalProjets,
            NombreEvenements = totalEvenements,
        };
    }

    /// <summary>
    /// Teinte de l'avatar, déduite du nom : deux membres voisins n'ont pas le même
    /// aplat, et une équipe tous uniformément colorée ressemblerait à une capture
    /// d'écran plutôt qu'à une page faite pour des personnes.
    /// </summary>
    private static string TeinteDe(string nom)
    {
        var somme = nom.Aggregate(0, (total, caractere) => total + caractere);
        return (somme % 4) switch { 0 => "violet", 1 => "vert", 2 => "orange", _ => "ink" };
    }

    private static ArticleVue EnVue(ArticleLu article) => new()
    {
        Titre = article.Titre,
        Categorie = article.Rubrique,
        Extrait = article.Extrait,
        Auteur = article.Auteur,
        FonctionAuteur = article.FonctionAuteur,
        LectureMinutes = DureeDeLecture(article.Contenu),
        Slug = article.Slug,
        Contenu = article.Contenu,
        DatePublication = article.PublieLeUtc is { } date
            ? DateOnly.FromDateTime(date)
            : DateOnly.FromDateTime(DateTime.Today),
        ImageUrl = article.ImageUrl,
    };

    private static EvenementVue EnVue(EvenementLu evenement) => new()
    {
        Titre = evenement.Titre,
        Slug = evenement.Slug,
        Type = evenement.Type,
        Lieu = evenement.Lieu,
        LienParticipation = evenement.LienParticipation,
        Resume = evenement.Resume,
        Description = evenement.Description,
        Contenu = evenement.Description,
        ImageUrl = evenement.ImageUrl,
        Format = evenement.Format,
        DateDebut = DateOnly.FromDateTime(evenement.DebutUtc),
        DateFin = DateOnly.FromDateTime(evenement.FinUtc),
    };

    private static RessourceVue EnVue(RessourceLu ressource) => new()
    {
        Titre = ressource.Titre,
        Categorie = ressource.Categorie,
        Sujet = ressource.Sujet,
        Description = ressource.Description,
        Lien = ressource.Lien ?? string.Empty,
        Externe = ressource.Externe,
        Auteur = ressource.Auteur,
    };

    private static ProjetVue EnVue(ProjetLu projet) => new()
    {
        Nom = projet.Nom,
        Description = projet.Description,
        Technologies = [.. projet.Technologies],
        Lien = projet.Lien,
    };

    private static PartenaireVue EnVue(PartenaireLu partenaire) => new()
    {
        Nom = partenaire.Nom,
        Type = partenaire.Type,
        Description = partenaire.Description,
        LogoUrl = partenaire.LogoUrl,
        Lien = partenaire.Lien,
    };

    /// <summary>
    /// Durée de lecture, à deux cents mots par minute — la moyenne de lecture silencieuse
    /// en français. Le nombre n'est qu'un repère, mais il doit être honnête.
    /// </summary>
    private static int DureeDeLecture(string contenu)
    {
        if (string.IsNullOrWhiteSpace(contenu))
        {
            return 1;
        }

        var mots = contenu.Count(caractere => char.IsLetterOrDigit(caractere));
        return Math.Max(1, (int)Math.Ceiling(mots / 200d));
    }
}
