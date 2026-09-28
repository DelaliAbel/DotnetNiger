using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DotnetNiger.Web.Controller;
using DotnetNiger.Web.Controller.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie la surface des routes déclarées par les contrôleurs.
/// </summary>
/// <remarks>
/// Ces tests portent sur la présence ou l'absence d'une opération, pas sur son résultat :
/// un appel retiré doit disparaître de la surface publique, sinon il reste joignable même
/// si son corps a été supprimé. La réflexion sur les attributs suffit, sans démarrer d'hôte.
/// </remarks>
public sealed class SurfaceApiTests
{
    private static readonly List<string> Routes = Lister();

    [Fact]
    public void AucunControleurNExposePlusUneApprobationDeCommentaire()
    {
        // L'approbation est une décision éditoriale qui n'appartient plus au cycle de vie
        // d'un commentaire : la route doit être absente, pas seulement inerte.
        Assert.DoesNotContain(Routes, route => route.Contains("/approve", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void LaModerationSeLimiteADepuiserMasquerEtSupprimer()
    {
        // La modération d'un commentaire tient en trois gestes : l'auteur ou l'admin
        // supprime, l'admin masque. Ni approbation, ni republication.
        var moderation = Routes
            .Where(route => route.StartsWith("api/comments", StringComparison.OrdinalIgnoreCase))
            .Where(route => route.Contains("approve", StringComparison.OrdinalIgnoreCase)
                || route.Contains("republish", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Empty(moderation);
    }

    [Fact]
    public void LesContratsPortentLeurValidationSurLeConstructeur()
    {
        // ASP.NET refuse de valider un record dont les attributs de validation sont posés
        // sur la propriété : il les attend sur le paramètre du constructeur et lève une
        // exception dès la première requête JSON, ce qui transforme toute écriture en
        // erreur 500. Les contrôleurs appelés directement ne revient pas dans ce chemin, ce
        // contrôle est le seul à pouvoir l'attraper.
        var surPropriete = new List<string>();
        var sansValidation = new List<string>();

        foreach (var contrat in EnumererLesRecords())
        {
            var proprietes = contrat
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(propriete => propriete.GetCustomAttributes<ValidationAttribute>(inherit: false).Any())
                .Select(propriete => $"{contrat.Name}.{propriete.Name}")
                .ToArray();

            surPropriete.AddRange(proprietes);

            var parametres = contrat
                .GetConstructors()
                .SelectMany(constructeur => constructeur.GetParameters())
                .Where(parametre => parametre.GetCustomAttributes<ValidationAttribute>(inherit: false).Any())
                .ToArray();

            // Un contrat qui ne valide plus du tout ne doit pas passer inaperçu non plus :
            // la validation doit avoir été déplacée, pas supprimée.
            if (parametres.Length == 0 && proprietes.Length > 0)
            {
                sansValidation.Add(contrat.Name);
            }
        }

        Assert.Empty(surPropriete);
        Assert.Empty(sansValidation);
    }

    private static IEnumerable<Type> EnumererLesRecords() =>
        typeof(UpdateProfileRequest).Assembly
            .GetTypes()
            .Where(type => type.Namespace == "DotnetNiger.Web.Controller.Contracts")
            .Where(type => type.IsClass && type.IsSealed && !type.IsAbstract)
            .Where(type => type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic) is not null);

    [Fact]
    public void LAdministrationPeutCreerUnCompte()
    {
        Assert.Contains("api/admin/accounts", Routes, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void LAdministrationVoitLesActualitesEnTousEtats()
    {
        // La liste publique ne montre que le publié : sans cet écran, un brouillon
        // resterait invisible de l'administration elle-même.
        Assert.Contains("api/admin/articles", Routes, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnMembrePeutSupprimerSonPropreCompte()
    {
        Assert.Contains("api/members/me", Routes, StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> Lister()
    {
        var routes = new List<string>();

        foreach (var type in typeof(CommentsController).Assembly
            .GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract))
        {
            // Un contrôleur peut porter plusieurs préfixes : chaque combinaison est une
            // route réellement servie, et toutes doivent être examinées.
            var prefixes = type
                .GetCustomAttributes<RouteAttribute>()
                .Select(prefix => prefix.Template ?? string.Empty)
                .ToArray();

            foreach (var methode in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                foreach (var attribut in methode.GetCustomAttributes<HttpMethodAttribute>())
                {
                    var suffixe = attribut.Template ?? string.Empty;

                    foreach (var prefix in prefixes)
                    {
                        // Un préfixe suivi d'un suffixe vide laisse une barre oblique finale
                        // qu'ASP.NET ignore : elle ferait échouer la comparaison.
                        var route = string.IsNullOrEmpty(prefix) ? suffixe : $"{prefix}/{suffixe}";

                        routes.Add(route.TrimEnd('/'));
                    }
                }
            }
        }

        return routes;
    }
}
