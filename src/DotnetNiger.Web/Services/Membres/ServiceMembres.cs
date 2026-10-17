using DotnetNiger.Application.Abstractions.Persistence;
using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Exceptions;
using DotnetNiger.Domain.Entities.Identity;
using DotnetNiger.Infrastructure.Identity;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace DotnetNiger.Web.Services.Membres;

/// <summary>Fiche publique d'un membre de la communauté.</summary>
/// <param name="Id">Identifiant du compte.</param>
/// <param name="Slug">Adresse de sa fiche.</param>
/// <param name="FullName">Nom affiché.</param>
/// <param name="Bio">Présentation.</param>
/// <param name="City">Ville.</param>
/// <param name="PhotoUrl">Photo de profil.</param>
/// <param name="Technologies">Compétences déclarées.</param>
/// <param name="IsTeam">Membre de l'équipe organisatrice.</param>
public sealed record MembreVue(
    Guid Id,
    string? Slug,
    string FullName,
    string? Bio,
    string? City,
    string? PhotoUrl,
    string? Technologies,
    bool IsTeam);

/// <summary>Lien externe affiché sur le profil d'un membre.</summary>
/// <param name="Id">Identifiant du lien.</param>
/// <param name="Label">Libellé visible.</param>
/// <param name="Url">Adresse HTTP ou HTTPS.</param>
/// <param name="SortOrder">Position dans la liste.</param>
public sealed record LienMembreVue(Guid Id, string Label, string Url, int SortOrder);

/// <summary>Valeur reçue du formulaire de liens membre.</summary>
public sealed class LienMembreInput
{
    /// <summary>Libellé du lien, par exemple GitHub ou Portfolio.</summary>
    public string? Label { get; set; }

    /// <summary>URL du lien.</summary>
    public string? Url { get; set; }
}

/// <summary>
/// Lecture et mise à jour de la fiche du membre connecté.
/// </summary>
/// <remarks>
/// L'annuaire publique a été retirée : la communauté se présente sur la page À propos,
/// et un profil éditable par son propriétaire suffit. Le service ne fait qu'appliquer les
/// règles du domaine — la normalisation des noms et le bornage des textes appartiennent à
/// l'entité, le retrait en cascade au service de suppression déjà en place.
/// </remarks>
public sealed class ServiceMembres(IUnitOfWork uniteDeTravail)
{
    // La fiche du membre connecté est demandée par la barre du haut et par la page, qui se
    // rendent en parallèle. Sans partage, deux lectures partageraient le même DbContext
    // concurrently — ce qu'EF refuse — et la page échouerait au premier rendu.
    // Chaque identifiant garde sa tâche : le second appelant attend le premier au lieu de
    // poser une seconde requête. La portée est celle du service, donc une requête HTTP ou
    // un circuit Blazor : la fiche n'est pas partagée d'un visiteur à l'autre.
    private readonly ConcurrentDictionary<Guid, Task<MembreVue?>> _lectures = new();

    /// <summary>
    /// Fiche d'un membre, par son identifiant de compte.
    /// </summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>La fiche, ou <see langword="null"/> si le compte n'existe pas.</returns>
    public Task<MembreVue?> LireAsync(Guid id, CancellationToken cancellationToken = default) =>
        _lectures.GetOrAdd(id, identifiant => LitAsync(identifiant, cancellationToken));

    /// <summary>
    /// Oublie la fiche mise en cache, pour qu'une modification soit lue au prochain rendu.
    /// </summary>
    /// <param name="id">Identifiant du compte modifié.</param>
    public void Oublie(Guid id) => _lectures.TryRemove(id, out _);

    /// <summary>Lit les liens externes d'un membre, dans l'ordre d'affichage.</summary>
    public async Task<IReadOnlyList<LienMembreVue>> ListerLiensAsync(
        Guid membreId,
        CancellationToken cancellationToken = default)
    {
        var liens = await uniteDeTravail.ReadRepository<MemberLink>().Query
            .AsNoTracking()
            .Where(lien => lien.MemberId == membreId)
            .OrderBy(lien => lien.SortOrder)
            .ThenBy(lien => lien.Id)
            .Select(lien => new LienMembreVue(lien.Id, lien.Label, lien.Url, lien.SortOrder))
            .ToListAsync(cancellationToken);

        if (liens.Count > 0)
        {
            return liens;
        }

        // Migration douce des anciens champs fixes : ils apparaissent comme des lignes
        // éditables tant qu'aucune collection moderne n'a été enregistrée. Au premier
        // enregistrement, RemplacerLiensAsync les copie dans MemberLinks et vide les
        // anciennes colonnes.
        var anciens = await uniteDeTravail.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .Where(membre => membre.Id == membreId)
            .Select(membre => new
            {
                membre.GitHubUsername,
                membre.LinkedInUrl,
                membre.XUrl,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (anciens is null)
        {
            return [];
        }

        var resultat = new List<LienMembreVue>();

        if (!string.IsNullOrWhiteSpace(anciens.GitHubUsername))
        {
            var identifiant = anciens.GitHubUsername.Trim().TrimStart('@');
            var url = identifiant.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? identifiant
                : $"https://github.com/{Uri.EscapeDataString(identifiant)}";
            resultat.Add(new LienMembreVue(Guid.Empty, "GitHub", url, resultat.Count));
        }

        if (!string.IsNullOrWhiteSpace(anciens.LinkedInUrl))
        {
            resultat.Add(new LienMembreVue(Guid.Empty, "LinkedIn", anciens.LinkedInUrl, resultat.Count));
        }

        if (!string.IsNullOrWhiteSpace(anciens.XUrl))
        {
            resultat.Add(new LienMembreVue(Guid.Empty, "X", anciens.XUrl, resultat.Count));
        }

        return resultat;
    }

    /// <summary>Remplace tous les liens externes d'un membre par sa nouvelle liste.</summary>
    /// <remarks>
    /// Les lignes vides du formulaire sont ignorées. Une ligne partiellement remplie est
    /// rejetée au lieu d'être silencieusement perdue. L'ordre transmis par le formulaire
    /// devient l'ordre public des liens.
    /// </remarks>
    public async Task RemplacerLiensAsync(
        Guid membreId,
        IReadOnlyCollection<LienMembreInput>? liens,
        CancellationToken cancellationToken = default)
    {
        var fournis = liens ?? [];
        var remplis = fournis
            .Where(lien => !string.IsNullOrWhiteSpace(lien.Label)
                || !string.IsNullOrWhiteSpace(lien.Url))
            .ToArray();

        if (remplis.Length > MemberLink.MaximumPerMember)
        {
            throw new DomainException(
                "too_many_member_links",
                $"Un profil peut présenter au plus {MemberLink.MaximumPerMember} liens.");
        }

        foreach (var lien in remplis)
        {
            if (string.IsNullOrWhiteSpace(lien.Label) || string.IsNullOrWhiteSpace(lien.Url))
            {
                throw new DomainException(
                    "member_link_incomplete",
                    "Chaque lien doit avoir un libellé et une adresse.");
            }
        }

        var existants = await uniteDeTravail.Repository<MemberLink>().TrackedQuery
            .Where(lien => lien.MemberId == membreId)
            .ToListAsync(cancellationToken);

        foreach (var existant in existants)
        {
            uniteDeTravail.Repository<MemberLink>().Remove(existant);
        }

        for (var index = 0; index < remplis.Length; index++)
        {
            var lien = remplis[index];
            uniteDeTravail.Repository<MemberLink>().Add(
                new MemberLink(membreId, lien.Label!, lien.Url!, index));
        }

        // Efface les anciennes colonnes, maintenant représentées par les MemberLinks
        // réellement enregistrés. Les autres champs du profil sont conservés.
        var membre = await uniteDeTravail.Repository<ApplicationUser>().TrackedQuery
            .FirstOrDefaultAsync(utilisateur => utilisateur.Id == membreId, cancellationToken);

        if (membre is not null)
        {
            membre.UpdateProfile(
                membre.FirstName,
                membre.LastName,
                membre.Profession,
                membre.Bio,
                membre.City,
                membre.PhotoUrl,
                gitHubUsername: null,
                linkedInUrl: null,
                xUrl: null,
                technologies: membre.Technologies);
        }

        await uniteDeTravail.SaveChangesAsync(cancellationToken);
    }

    private async Task<MembreVue?> LitAsync(Guid id, CancellationToken cancellationToken)
    {
        var membre = await uniteDeTravail.ReadRepository<ApplicationUser>().Query
            .AsNoTracking()
            .Where(membre => membre.Id == id)
            .Select(membre => new MembreVue(
                membre.Id,
                membre.Slug,
                membre.FullName,
                membre.Bio,
                membre.City,
                // Une photo déposée l'emporte sur l'adresse saisie à la main : c'est la
                // photo que le membre vient d'envoyer, pas une ancienne qu'il a oubliée.
                // L'adresse n'est pas réécrite en base — la ligne ProfilePhotos reste
                // l'unique source de vérité.
                uniteDeTravail.ReadRepository<ProfilePhoto>().Query
                    .Where(photo => photo.MembreId == membre.Id)
                    .Select(photo => photo.Id)
                    .Any()
                    ? $"/api/photos-profil/{membre.Id}"
                    : membre.PhotoUrl,
                membre.Technologies,
                membre.IsTeam))
            .FirstOrDefaultAsync(cancellationToken);

        return membre;
    }

    /// <summary>
    /// Enregistre les modifications du profil, et renvoie la liste des champs refusés.
    /// </summary>
    /// <param name="id">Identifiant du compte.</param>
    /// <param name="prenom">Prénom.</param>
    /// <param name="nom">Nom de famille.</param>
    /// <param name="bio">Présentation.</param>
    /// <param name="ville">Ville.</param>
    /// <param name="photoUrl">Adresse de la photo.</param>
    /// <param name="github">Compte GitHub.</param>
    /// <param name="linkedin">Adresse LinkedIn.</param>
    /// <param name="x">Adresse X.</param>
    /// <param name="technologies">Compétences.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns><see langword="true"/> si le profil a été enregistré.</returns>
    public async Task<bool> MettreAJourAsync(
        Guid id,
        string prenom,
        string nom,
        string? bio,
        string? ville,
        string? photoUrl,
        string? github,
        string? linkedin,
        string? x,
        string? technologies,
        CancellationToken cancellationToken = default)
    {
        var membre = await uniteDeTravail.ReadRepository<ApplicationUser>().Query
            .FirstOrDefaultAsync(utilisateur => utilisateur.Id == id, cancellationToken);

        if (membre is null)
        {
            return false;
        }

        // Le domaine normalise les noms, taille les textes et juge les adresses : il
        // refuse les valeurs fautives d'un message, là où une page ne le peut pas.
        membre.UpdateProfile(
            prenom.Trim(),
            nom.Trim(),
            // La fonction appartient à l'administration : c'est elle qui coche la case
            // « membre de l'équipe » et saisit le poste. Elle est renvoyée telle quelle, et
            // non omise — sans cet argument, le membre qui enregistrait sa biographie depuis
            // son espace effaçait sa propre fonction, et la carte « À propos » perdait son
            // poste sans qu'aucun écran ne signale quoi que ce soit.
            membre.Profession,
            bio,
            ville,
            // Le champ d'adresse n'est pas éditable : les photos viennent du fournisseur
            // d'identité ou du dépôt de fichier. L'absence de PhotoUrl dans le formulaire
            // doit donc conserver l'avatar Google existant, pas l'effacer.
            photoUrl ?? membre.PhotoUrl,
            github ?? membre.GitHubUsername,
            linkedin ?? membre.LinkedInUrl,
            x ?? membre.XUrl,
            technologies);

        uniteDeTravail.Repository<ApplicationUser>().Update(membre);
        await uniteDeTravail.SaveChangesAsync(cancellationToken);

        // La fiche lue en cache décrit la version d'avant : sans cette invalidation, la
        // page profilerait l'ancien nom jusqu'à la fin du circuit.
        Oublie(id);

        return true;
    }
}
