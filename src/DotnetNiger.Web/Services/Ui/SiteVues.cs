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

    public required string Description { get; init; }

    public string? ImageUrl { get; init; }

    /// <summary>Premier jour de l'événement. Sans elle, impossible de distinguer un meetup à venir d'un hackathon passé.</summary>
    public required DateOnly DateDebut { get; init; }

    /// <summary>Dernier jour, pour un événement qui dure plus d'un jour ; nul pour une soirée ou un atelier.</summary>
    public DateOnly? DateFin { get; init; }
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

    public required string Description { get; init; }

    public required string[] Technologies { get; init; }

    public required int Etoiles { get; init; }

    public required int Contributeurs { get; init; }

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

    public string? LienGithub { get; init; }

    public string? LienLinkedin { get; init; }

    public string? LienX { get; init; }
}

/// <summary>Modèle de présentation d'un lien vers un réseau social ou un canal de la communauté.</summary>
public sealed class CanalVue
{
    public required string Nom { get; init; }

    public required string Url { get; init; }

    public required string Icône { get; init; }
}

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

    public required IReadOnlyList<CanalVue> Canaux { get; init; } = Array.Empty<CanalVue>();

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

    public required IReadOnlyList<CanalVue> Canaux { get; init; } = Array.Empty<CanalVue>();

    public required int NombreMembres { get; init; }

    public required int NombreProjets { get; init; }

    public required int NombreEvenements { get; init; }

    public required int NombreCanaux { get; init; }
}