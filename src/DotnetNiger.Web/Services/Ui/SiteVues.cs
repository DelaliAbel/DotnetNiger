namespace DotnetNiger.Web.Services.Ui;

/// <summary>Modèle de présentation d'un article de blog.</summary>
public sealed class ArticleVue
{
    public required string Titre { get; init; }

    public required string Categorie { get; init; }

    public required string Extrait { get; init; }

    public required string Auteur { get; init; }

    public required int LectureMinutes { get; init; }

    public required string Slug { get; init; }

    public string? ImageUrl { get; init; }
}

/// <summary>Modèle de présentation d'un événement (meetup, conférence, hackathon…).</summary>
public sealed class EvenementVue
{
    public required string Titre { get; init; }

    public required string Type { get; init; }

    public required string Lieu { get; init; }

    public required string Description { get; init; }

    public string? ImageUrl { get; init; }
}

/// <summary>Modèle de présentation d'une ressource (formation, tutoriel, guide, outil).</summary>
public sealed class RessourceVue
{
    public required string Titre { get; init; }

    public required string Categorie { get; init; }

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
}

/// <summary>Modèle de présentation d'un lien vers un réseau social ou un canal de la communauté.</summary>
public sealed class CanalVue
{
    public required string Nom { get; init; }

    public required string Url { get; init; }

    public required string Icône { get; init; }
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