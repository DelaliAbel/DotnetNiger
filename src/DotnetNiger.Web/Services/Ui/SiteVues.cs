using DotnetNiger.Domain.Enums;

namespace DotnetNiger.Web.Services.Ui;

/// <summary>Modèle de présentation d'un article de blog.</summary>
public sealed class ArticleVue
{
    public required string Titre { get; init; }

    public required string Categorie { get; init; }

    public required string Extrait { get; init; }

    public required string Auteur { get; init; }

    /// <summary>
    /// Titre professionnel de l'auteur (« Développeur backend », « Formatrice C# »).
    /// Facultatif : certains articles sont publiés sous une signature collective, et
    /// afficher une mention générique à leur place n'apporterait rien.
    /// </summary>
    public string? FonctionAuteur { get; init; }

    public required int LectureMinutes { get; init; }

    public required string Slug { get; init; }

    /// <summary>
    /// Corps de l'article, un paragraphe par bloc séparé par une ligne vide. Du texte et non
    /// du HTML : la garniture n'a pas vocation à rendre une balise, et la lecture de l'article
    /// n'a pas à s'appuyer sur du balisage produit ailleurs.
    /// </summary>
    public required string Contenu { get; init; }

    /// <summary>Date de publication, affichée sur la carte et en tête de l'article.</summary>
    public required DateOnly DatePublication { get; init; }

    public string? ImageUrl { get; init; }
}

/// <summary>Modèle de présentation d'un événement (meetup, conférence, hackathon…).</summary>
public sealed class EvenementVue
{
    public required string Titre { get; init; }

    /// <summary>Identifiant d'URL de la fiche, nécessaire à la route /evenements/{slug}.</summary>
    public required string Slug { get; init; }

    public required string Type { get; init; }

    public required string Lieu { get; init; }

    /// <summary>
    /// Adresse de participation d'un événement en ligne ou hybride.
    /// </summary>
    /// <remarks>
    /// Exigée par le domaine dès que le format est en ligne ou hybride, et indispensable
    /// à l'inscription : c'est elle que reçoit le membre qui réserve sa place. Un événement
    /// en présentiel seul n'en a pas besoin.
    /// </remarks>
    public string? LienParticipation { get; init; }

    public required string Description { get; init; }

    /// <summary>
    /// Accroche courte de la rencontre, une ou deux phrases.
    /// </summary>
    /// <remarks>
    /// Le résumé et la description ne disent pas la même chose : le premier donne envie
    /// de venir, la seconde raconte ce qui s'est passé. Confondre les deux ferait
    /// perdre la moitié du compte rendu d'un meetup.
    /// </remarks>
    public string Resume { get; init; } = string.Empty;

    /// <summary>
    /// Corps complet de la rencontre, en HTML.
    /// </summary>
    /// <remarks>
    /// Le domaine stocke le corps en HTML, assaini à l'écriture. Résumé et description
    /// n'en sont que des extraits : afficher la description comme si elle était le corps
    /// ferait perdre la moitié du compte rendu d'un meetup.
    /// </remarks>
    public string Contenu { get; init; } = string.Empty;

    public string? ImageUrl { get; init; }

    /// <summary>Premier jour de l'événement. Sans elle, impossible de distinguer un meetup à venir d'un hackathon passé.</summary>
    public required DateOnly DateDebut { get; init; }

    /// <summary>Dernier jour, pour un événement qui dure plus d'un jour ; nul pour une soirée ou un atelier.</summary>
    public DateOnly? DateFin { get; init; }

    /// <summary>
    /// Modalité de la rencontre : présentiel, en ligne ou hybride.
    /// </summary>
    /// <remarks>
    /// Affichée sur les cartes, où elle répond à la première question de qui lit : peut-on
    /// venir, et depuis où ? Le lieu seul ne le dit pas — une rencontre en ligne n'a pas
    /// de lieu, et une hybride en a un sans que ce soit l'essentiel.
    /// </remarks>
    public CommunityEventFormat Format { get; init; }
}

/// <summary>Modèle de présentation d'une ressource (formation, tutoriel, guide, outil).</summary>
public sealed class RessourceVue
{
    public required string Titre { get; init; }

    public required string Categorie { get; init; }

    /// <summary>Étiquette technique (ex. « Blazor », « ASP.NET Core », « MAUI »).</summary>
    public string Sujet { get; init; } = "";

    public required string Description { get; init; }

    public required string Lien { get; init; }

    public required bool Externe { get; init; } = true;

    public required string Auteur { get; init; }
}

/// <summary>Modèle de présentation d'un projet open source de la communauté.</summary>
public sealed class ProjetVue
{
    public required string Nom { get; init; }

    /// <summary>
    /// Phrase d'accroche du projet, en texte brut.
    /// </summary>
    /// <remarks>
    /// La description détaillée du projet est du HTML et n'est pas portée ici : les cartes
    /// renvoient au dépôt, où le projet se lit. Ce champ reçoit le résumé, pas la
    /// description — le nom est trompeur, mais il est partagé avec l'API publique.
    /// </remarks>
    public required string Description { get; init; }

    public required string[] Technologies { get; init; }

    public required string Lien { get; init; }
}

/// <summary>Modèle de présentation d'un partenaire ou sponsor.</summary>
public sealed class PartenaireVue
{
    public required string Nom { get; init; }

    public required string Type { get; init; }

    public required string Description { get; init; }

    public string? LogoUrl { get; init; }

    public string? Lien { get; init; }
}

/// <summary>Modèle de présentation d'un membre de la communauté.</summary>
public sealed class MembreVue
{
    public required string Nom { get; init; }

    public required string Role { get; init; }

    public required string Teinte { get; init; } = "violet";

    public string? ImageUrl { get; init; }

    /// <summary>
    /// Liens affichés sur la fiche, dans l'ordre choisi par le membre.
    /// </summary>
    /// <remarks>
    /// Une liste, et non trois champs fixes : le membre en ajoute autant qu'il veut depuis
    /// son profil, et c'est cette liste qu'il a lui-même remplie qui est présentée ici.
    /// Trois colonnes figées ne pouvaient porter qu'une adresse par réseau — et aucun
    /// écran du site ne les remplissait.
    /// </remarks>
    public IReadOnlyList<LienEquipeVue> Liens { get; init; } = [];
}

/// <summary>Lien externe présenté sur la fiche d'un membre.</summary>
/// <param name="Libelle">Nom du lien, saisit par le membre.</param>
/// <param name="Url">Adresse de destination.</param>
/// <param name="Icone">
/// Nom de l'icône à dessiner. Le composant ne connaît que ces noms : un réseau non
/// reconnu prend l'icône générique plutôt que de ne rien afficher du tout.
/// </param>
public sealed record LienEquipeVue(string Libelle, string Url, string Icone);

/// <summary>Modèle de présentation d'un jalon de l'histoire de la communauté.</summary>
public sealed class JalonVue
{
    /// <summary>Période affichée (ex. « 2021 · 2022 » ou « 2024 »).</summary>
    public required string Periode { get; init; }

    public required string Titre { get; init; }

    public required string Description { get; init; }
}

/// <summary>Modèle de présentation d'une valeur fondamentale de la communauté.</summary>
public sealed class ValeurVue
{
    public required string Titre { get; init; }

    public required string Description { get; init; }

    public required string Icone { get; init; }
}

/// <summary>Données d'affichage de la page À propos.</summary>
public sealed class AProposVue
{
    /// <summary>Texte long de présentation (« Qui sommes-nous ? »).</summary>
    public required string Presentation { get; init; }

    /// <summary>Titre de la mission (slogan).</summary>
    public required string Mission { get; init; }

    /// <summary>Paragraphe qui développe la mission.</summary>
    public required string MissionDetail { get; init; }

    public required IReadOnlyList<ValeurVue> Valeurs { get; init; } = Array.Empty<ValeurVue>();

    public required IReadOnlyList<JalonVue> Histoire { get; init; } = Array.Empty<JalonVue>();

    public required IReadOnlyList<MembreVue> Equipe { get; init; } = Array.Empty<MembreVue>();
}

/// <summary>Données d'affichage de la page d'accueil.</summary>
public sealed class AccueilVue
{
    public required IReadOnlyList<ArticleVue> ArticlesRecents { get; init; } = Array.Empty<ArticleVue>();

    public required IReadOnlyList<EvenementVue> EvenementsAVenir { get; init; } = Array.Empty<EvenementVue>();

    public required IReadOnlyList<RessourceVue> RessourcesPopulaires { get; init; } = Array.Empty<RessourceVue>();

    public required int NombreMembres { get; init; }

    public required int NombreProjets { get; init; }

    public required int NombreRessources { get; init; }

    public required int NombreEvenements { get; init; }
}

/// <summary>Données d'affichage de la page Communauté.</summary>
public sealed class CommunauteVue
{
    public required IReadOnlyList<EvenementVue> Evenements { get; init; } = Array.Empty<EvenementVue>();

    public required IReadOnlyList<ProjetVue> Projets { get; init; } = Array.Empty<ProjetVue>();

    public required int NombreMembres { get; init; }

    public required int NombreProjets { get; init; }

    public required int NombreEvenements { get; init; }
}