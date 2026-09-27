using System.Reflection;

namespace DotnetNiger.ArchitectureTests;

/// <summary>
/// Vérifie que les dépendances entre projets vont dans un seul sens.
///
/// Une architecture en couches perd sa valeur dès qu'une couche basse référence une
/// couche haute : le domaine devient impossible à tester seul, et une dépendance
/// technique comme ASP.NET Identity se glisse dans le modèle métier. Ces tests
/// échouent au moment du build plutôt qu'au moment d'un bug difficile à diagnostiquer.
/// </summary>
public sealed class LayerDependencyTests
{
    private const string Domain = "DotnetNiger.Domain";
    private const string Application = "DotnetNiger.Application";
    private const string Infrastructure = "DotnetNiger.Infrastructure";
    private const string Web = "DotnetNiger.Web";

    public static TheoryData<string, string[]> DependancesInterdites => new()
    {
        { Domain, [Application, Infrastructure, Web] },
        { Application, [Infrastructure, Web] },
        { Infrastructure, [Web] },
    };

    [Theory]
    [MemberData(nameof(DependancesInterdites))]
    public void Une_couche_ne_reference_pas_les_couches_superieures(string couche, string[] interdites)
    {
        var assembly = ChargerAssembly(couche);
        var references = assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        var violations = interdites
            .Where(references.Contains)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            $"{couche} ne doit pas référencer {string.Join(", ", violations)}.");
    }

    [Fact]
    public void Les_quatre_couches_sont_referencees_par_les_tests_d_architecture()
    {
        // Garde-fou : si un projet disparaît ou change de nom, ce test le signale au lieu
        // de laisser `ChargerAssembly` échouer sur une erreur incompréhensible.
        foreach (var couche in new[] { Domain, Application, Infrastructure, Web })
        {
            Assert.NotNull(ChargerAssembly(couche));
        }
    }

    [Fact]
    public void Le_domaine_ne_depend_D_ASP_Net()
    {
        var references = ChargerAssembly(Domain).GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty);

        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    private static Assembly ChargerAssembly(string nom)
    {
        return nom switch
        {
            Domain => typeof(DotnetNiger.Domain.Entities.Content.Article).Assembly,
            Application => typeof(DotnetNiger.Application.Abstractions.Persistence.IUnitOfWork).Assembly,
            Infrastructure => typeof(DotnetNiger.Infrastructure.Persistence.DotnetNigerDbContext).Assembly,
            Web => typeof(DotnetNiger.Web.Components.App).Assembly,
            _ => throw new ArgumentOutOfRangeException(nameof(nom), nom, "Couche inconnue."),
        };
    }
}
