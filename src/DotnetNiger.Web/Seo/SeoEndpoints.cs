using System.Globalization;
using System.Text;
using System.Xml.Linq;
using DotnetNiger.Application.Abstractions;
using DotnetNiger.Web.Services.Ui;
using Microsoft.Extensions.Options;

namespace DotnetNiger.Web.Seo;

/// <summary>
/// Plan du site et directives d'indexation, servis au format attendu par les moteurs.
/// </summary>
/// <remarks>
/// Un moteur ne découvre pas ce qu'on ne lui déclare pas. Les pages publiques sont peu
/// nombreuses et stables, mais elles restent à deux liens ou plus de la page d'accueil :
/// sans plan, un article publié depuis six mois n'apparaît qu'au bout du parcours.
/// <para>
/// Les adresses viennent de la configuration du serveur, comme les liens des courriels et
/// les balises de partage. Un plan écrit à la main conserverait l'ancien domaine après un
/// changement, et le moteur serait invité à y porter son attention pour rien.
/// </para>
/// </remarks>
internal static class SeoEndpoints
{
    private const string EspaceDeNoms = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private static readonly XNamespace Schema = EspaceDeNoms;

    /// <summary>Déclare le plan du site et le fichier de directives.</summary>
    /// <param name="app">Application en cours de construction.</param>
    /// <returns>Le même application, pour permettre le chaînage.</returns>
    public static WebApplication MapSeoEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGet("/robots.txt", (IOptions<DotnetNiger.Infrastructure.Email.EmailOptions> options) =>
            {
                var racine = options.Value.SiteUrl.TrimEnd('/');

                var texte = new StringBuilder()
                    .AppendLine("User-agent: *")
                    .AppendLine("Allow: /")
                    // L'espace membre et l'administration n'ont rien à faire dans un index :
                    // ils exigent une session, et leurs adresses ne doivent pas être proposées.
                    .AppendLine("Disallow: /administration")
                    .AppendLine("Disallow: /compte")
                    .AppendLine("Disallow: /api/")
                    .AppendLine()
                    .AppendLine(CultureInfo.InvariantCulture, $"Sitemap: {racine}/sitemap.xml")
                    .ToString();

                return Results.Text(texte, "text/plain; charset=utf-8");
            })
            .AllowAnonymous()
            .ExcludeFromDescription();

        app.MapGet("/sitemap.xml", async (
            IOptions<DotnetNiger.Infrastructure.Email.EmailOptions> options,
            ISiteContentService contenu,
            CancellationToken annulation) =>
            {
                var racine = options.Value.SiteUrl.TrimEnd('/');

                var plan = new XElement(
                    Schema + "urlset",
                    // Le namespace du plan se déclare par son préfixe, pas par un attribut
                    // « xmlns » sans nom : .NET refuse un nom local vide, et le document
                    // produit serait refusé par les moteurs.
                    new XAttribute(XNamespace.Xmlns + "sitemap", EspaceDeNoms),
                    Page(racine, "/", 1.0),
                    Page(racine, "/blog", 0.9),
                    Page(racine, "/evenements", 0.9),
                    Page(racine, "/calendrier", 0.6),
                    Page(racine, "/ressources", 0.9),
                    Page(racine, "/communaute", 0.8),
                    Page(racine, "/partenaires", 0.6),
                    Page(racine, "/a-propos", 0.5),
                    Page(racine, "/contact", 0.5));

                // Les contenus publiés sont énumérés à part : une liste figée finirait par
                // annoncer des adresses retirées, ce qui est pire que de ne rien annoncer.
                foreach (var article in (await contenu.ChargerArticlesAsync(annulation)).Take(1000))
                {
                    plan.Add(Page(racine, $"/blog/{article.Slug}", 0.7, article.DatePublication));
                }

                foreach (var evenement in (await contenu.ChargerEvenementsAsync(annulation)).Take(500))
                {
                    plan.Add(Page(racine, $"/evenements/{evenement.Slug}", 0.7));
                }

                var document = new XDocument(plan);

                return Results.Text(
                    document.ToString(SaveOptions.DisableFormatting),
                    "application/xml; charset=utf-8");
            })
            .AllowAnonymous()
            .ExcludeFromDescription();

        return app;
    }

    private static XElement Page(
        string racine,
        string chemin,
        double priorite,
        DateOnly? miseAJour = null)
    {
        var url = new XElement(
            Schema + "loc",
            $"{racine}/{chemin.TrimStart('/')}");

        return miseAJour is { } date
            ? new XElement(
                Schema + "url",
                url,
                new XElement(
                    Schema + "lastmod",
                    date.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                new XElement(Schema + "priority", priorite.ToString("0.0", CultureInfo.InvariantCulture)))
            : new XElement(
                Schema + "url",
                url,
                new XElement(Schema + "priority", priorite.ToString("0.0", CultureInfo.InvariantCulture)));
    }
}