using DotnetNiger.Application.Abstractions;
using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DotnetNiger.Web.Services.Reglages;

/// <summary>Un canal de la communauté, prêt à être affiché.</summary>
/// <param name="Nom">Nom du canal, pour l'accessibilité et l'infobulle.</param>
/// <param name="Url">Adresse du canal.</param>
/// <param name="Icone">Nom d'icône dans la bibliothèque du site.</param>
public sealed record CanalSite(string Nom, string Url, string Icone);

/// <summary>Les réglages du site, tels que l'administration les modifie.</summary>
/// <param name="Logo">Logo du site, en-tête et courriels, ou <see langword="null"/>.</param>
/// <param name="Courriel">Adresse de contact, ou <see langword="null"/>.</param>
/// <param name="Whatsapp">Adresse du groupe WhatsApp, ou <see langword="null"/>.</param>
/// <param name="Github">Dépôt GitHub, ou <see langword="null"/>.</param>
/// <param name="Linkedin">Page LinkedIn, ou <see langword="null"/>.</param>
/// <param name="X">Compte X, ou <see langword="null"/>.</param>
/// <param name="Youtube">Chaîne YouTube, ou <see langword="null"/>.</param>
/// <param name="Facebook">Page Facebook, ou <see langword="null"/>.</param>
public sealed record ReglagesVue(
    string? Logo,
    string? Courriel,
    string? Whatsapp,
    string? Github,
    string? Linkedin,
    string? X,
    string? Youtube,
    string? Facebook);

/// <summary>Les mêmes réglages, avec la liste des canaux à afficher.</summary>
/// <param name="Valeurs">Réglages bruts.</param>
/// <param name="Canaux">Canaux renseignés, dans l'ordre d'affichage.</param>
public sealed record ReglagesAffiches(ReglagesVue Valeurs, IReadOnlyList<CanalSite> Canaux);

/// <summary>
/// Lecture et écriture des réglages du site.
/// </summary>
/// <remarks>
/// Un réglage absent ne se distingue pas d'un réglage vide : les deux signifient « ne rien
/// afficher ». C'est la règle qui évite qu'un lien non renseigné apparaisse dans le pied de
/// page sous la forme d'une icône qui mène nulle part.
/// <para>
/// La lecture est mise en cache tant que l'administration n'écrit pas. Le pied de page est
/// rendu sur chaque page : sans cela, chaque visite coûterait une requête pour rien.
/// </para>
/// </remarks>
public sealed class ServiceReglages(
    IServiceScopeFactory fabriquePortee,
    IMemoryCache cache)
{
    /// <summary>Clé de cache des réglages.</summary>
    private const string CleCache = "reglages.site";

    /// <summary>Lit les réglages, une seule fois par changement.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les réglages du site.</returns>
    /// <remarks>
    /// Le service est unique et ouvre sa propre portée pour lire. Il ne doit surtout pas
    /// partager le contexte de la requête qui l'appelle : le pied de page et la marque sont
    /// initialisés en même temps que la page, et un <c>DbContext</c> ne supporte qu'une
    /// seule opération à la fois. Partagé, le premier rendu du site échouait sur une
    /// « seconde opération démarrée ».
    /// </remarks>
    public async Task<ReglagesVue> LireAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CleCache, out ReglagesVue? memorise) && memorise is not null)
        {
            return memorise;
        }

        var valeurs = await LireInterneAsync(cancellationToken);
        cache.Set(CleCache, valeurs);

        return valeurs;
    }

    /// <summary>Les réglages et les canaux effectivement affichables.</summary>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Les réglages et la liste des canaux renseignés.</returns>
    public async Task<ReglagesAffiches> LirePourAffichageAsync(CancellationToken cancellationToken = default)
    {
        var valeurs = await LireAsync(cancellationToken);

        return new ReglagesAffiches(valeurs, Canaux(valeurs));
    }

    /// <summary>Remplace la valeur d'un réglage. Une valeur vide retire le réglage.</summary>
    /// <param name="cle">Clé du réglage.</param>
    /// <param name="valeur">Nouvelle valeur.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Le résultat de l'écriture.</returns>
    public async Task<(bool Reussi, string Message)> DefinirAsync(
        string cle,
        string? valeur,
        CancellationToken cancellationToken = default)
    {
        using var portee = fabriquePortee.CreateScope();
        var uniteDeTravail = portee.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Un réglage retiré disparaît de la table plutôt que d'y rester vide : la liste
        // des réglages du site est alors exactement la liste de ce qui est configuré.
        if (string.IsNullOrWhiteSpace(valeur))
        {
            var aRetirer = await uniteDeTravail.ReadRepository<ReglageSite>().Query
                .FirstOrDefaultAsync(reglage => reglage.Cle == cle, cancellationToken);

            if (aRetirer is not null)
            {
                uniteDeTravail.Repository<ReglageSite>().Remove(aRetirer);
            }
        }
        else
        {
            var existant = await uniteDeTravail.ReadRepository<ReglageSite>().Query
                .FirstOrDefaultAsync(reglage => reglage.Cle == cle, cancellationToken);

            if (existant is null)
            {
                uniteDeTravail.Repository<ReglageSite>().Add(new ReglageSite(cle, valeur));
            }
            else
            {
                existant.Definir(valeur);
                uniteDeTravail.Repository<ReglageSite>().Update(existant);
            }
        }

        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        Oublie();

        return (true, "Réglages enregistrés.");
    }

    /// <summary>Relit les réglages à la prochaine demande, après une écriture.</summary>
    public void Oublie() => cache.Remove(CleCache);

    /// <summary>
    /// Canaux renseignés, dans l'ordre d'affichage.
    /// </summary>
    /// <param name="valeurs">Réglages lus.</param>
    /// <returns>Les canaux dont l'adresse est renseignée.</returns>
    private static IReadOnlyList<CanalSite> Canaux(ReglagesVue valeurs) =>
    [
        .. new (string? Url, CanalSite Canal)[]
        {
            (valeurs.Whatsapp, new CanalSite("WhatsApp", "#", "whatsapp")),
            (valeurs.Github, new CanalSite("GitHub", "#", "github")),
            (valeurs.Linkedin, new CanalSite("LinkedIn", "#", "linkedin")),
            (valeurs.X, new CanalSite("X (Twitter)", "#", "x")),
            (valeurs.Youtube, new CanalSite("YouTube", "#", "youtube")),
            (valeurs.Facebook, new CanalSite("Facebook", "#", "facebook")),
        }
            .Where(canal => !string.IsNullOrWhiteSpace(canal.Url))
            .Select(canal => canal.Canal with { Url = canal.Url! }),
    ];

    private async Task<ReglagesVue> LireInterneAsync(CancellationToken cancellationToken)
    {
        using var portee = fabriquePortee.CreateScope();
        var uniteDeTravail = portee.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var reglages = await uniteDeTravail.ReadRepository<ReglageSite>().Query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        string? Valeur(string cle)
            => reglages.FirstOrDefault(reglage => reglage.Cle == cle)?.Valeur;

        return new ReglagesVue(
            Valeur(ClesReglage.Logo),
            Valeur(ClesReglage.Courriel),
            Valeur(ClesReglage.Whatsapp),
            Valeur(ClesReglage.Github),
            Valeur(ClesReglage.Linkedin),
            Valeur(ClesReglage.X),
            Valeur(ClesReglage.Youtube),
            Valeur(ClesReglage.Facebook));
    }
}