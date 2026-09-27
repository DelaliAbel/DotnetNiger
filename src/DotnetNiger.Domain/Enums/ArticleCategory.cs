namespace DotnetNiger.Domain.Enums;

/// <summary>
/// Categories du blog, telles que decrites dans la specification.
/// </summary>
public enum ArticleCategory
{
    /// <summary>Annonces des nouvelles versions de .NET et de leurs fonctionnalites.</summary>
    DotNetNews = 0,

    /// <summary>Mise a jour des projets .NET developpes par la communaute.</summary>
    LocalProjects = 1,

    /// <summary>Entretiens avec des developpeurs locaux ou internationaux.</summary>
    Interview = 2,
}
