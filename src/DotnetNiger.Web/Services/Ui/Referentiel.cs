namespace DotnetNiger.Web.Services.Ui;

/// <summary>
/// Référentiel unique des rubriques éditoriales, des catégories de ressources et des
/// types d'événements.
/// </summary>
/// <remarks>
/// Ces trois listes vivaient dans les pages qui les affichent, chacune avec sa propre
/// paire (clé, nom). Une rubrique typée dans un article pouvait alors ne correspondre
/// à aucun filtre, et le reconstructeur d'URL de la fiche article produisait un lien
/// qui ne filtrait plus rien. Le référentiel sert désormais les trois pages, et il est
/// la source naturelle du menu déroulant de la rédaction d'article : la rubrique
/// proposée à l'auteur est exactement celle que verra le lecteur.
/// </remarks>
public static class Referentiel
{
    /// <summary>Rubrique éditoriale d'un article.</summary>
    /// <param name="Slug">Clé utilisée dans les URL de filtre.</param>
    /// <param name="Nom">Libellé affiché et stocké sur l'article.</param>
    /// <param name="Couleur">Variante de badge (<c>violet</c>, <c>vert</c>, <c>orange</c>, <c>ink</c>).</param>
    public sealed record Rubrique(string Slug, string Nom, string Couleur);

    /// <summary>
    /// Rubriques du blog. Les trois premières sont historiques et conservent leur slug
    /// d'origine («?categorie=projets» reste valide).
    /// </summary>
    public static IReadOnlyList<Rubrique> RubriquesArticle { get; } =
    [
        new("nouveautes", "Nouveautés de .NET", "violet"),
        new("csharp", "C#", "violet"),
        new("blazor", "Blazor", "violet"),
        new("aspnet-core", "ASP.NET Core", "violet"),
        new("architecture", "Architecture & performance", "orange"),
        new("securite", "Sécurité", "orange"),
        new("devops", "DevOps & déploiement", "ink"),
        new("donnees", "Données & Entity Framework", "vert"),
        new("tests", "Tests & qualité", "vert"),
        new("outils", "Outils & bibliothèques", "vert"),
        new("interviews", "Interviews", "orange"),
        new("projets", "Projets locaux", "vert"),
        new("retours", "Retours d'expérience", "ink"),
    ];

    /// <summary>
    /// Catégories de la bibliothèque de ressources. Chacune possède au moins une
    /// entrée : cette bibliothèque est sélectionnée à la main, une catégorie vide
    /// n'y aurait pas de sens.
    /// </summary>
    public static IReadOnlyList<Rubrique> CategoriesRessource { get; } =
    [
        new("formations", "Formations et tutoriels", "violet"),
        new("articles", "Articles de blog et guides", "violet"),
        new("outils", "Bibliothèques et outils", "vert"),
        new("documentation", "Documentation .NET", "vert"),
        new("securite", "Sécurité et identité", "orange"),
        new("devops", "DevOps et déploiement", "ink"),
        new("tests", "Tests et qualité", "vert"),
        new("donnees", "Données et persistance", "vert"),
    ];

    /// <summary>Types de rendez-vous de l'agenda.</summary>
    public static IReadOnlyList<Rubrique> TypesEvenement { get; } =
    [
        new("meetup", "Meetup", "violet"),
        new("conference", "Conférence", "orange"),
        new("atelier", "Atelier", "vert"),
        new("hackathon", "Hackathon", "orange"),
        new("table-ronde", "Table ronde", "ink"),
        new("webinaire", "Webinaire", "vert"),
        new("mentorat", "Session de mentorat", "ink"),
        new("rencontre", "Rencontre régionale", "vert"),
    ];

    /// <summary>Rubrique d'article correspondant à un slug de filtre, ou <c>null</c>.</summary>
    public static Rubrique? RubriqueArticleParSlug(string? slug) =>
        RubriquesArticle.FirstOrDefault(r => r.Slug == slug);

    /// <summary>Rubrique d'article correspondant au libellé stocké sur l'article, ou <c>null</c>.</summary>
    public static Rubrique? RubriqueArticleParNom(string? nom) =>
        RubriquesArticle.FirstOrDefault(r => r.Nom == nom);

    /// <summary>Catégorie de ressource correspondant à un slug de filtre, ou <c>null</c>.</summary>
    public static Rubrique? CategorieRessourceParSlug(string? slug) =>
        CategoriesRessource.FirstOrDefault(c => c.Slug == slug);

    /// <summary>Type d'événement correspondant à un slug de filtre, ou <c>null</c>.</summary>
    public static Rubrique? TypeEvenementParSlug(string? slug) =>
        TypesEvenement.FirstOrDefault(t => t.Slug == slug);
}
