using System.Globalization;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Met en forme les dates et les lieux pour un message en français.
/// </summary>
/// <remarks>
/// Les modèles sont rédigés en français, à destination de lecteurs francophones : la date
/// s'écrit donc « samedi 18 mars 2026 à 16 h », et non « 03/18/2026 16:00:00 ». C'est un
/// choix de langue assumé, pas une dépendance implicite à la culture du serveur : la
/// culture est nommée, pour qu'un déploiement change de langue système n'altère pas les
/// messages.
/// <para>
/// Les dates sont rendues en heure de Niamey, qui est UTC+0 toute l'année : aucun décalage
/// n'est appliqué, et la conversion reste sans effet aujourd'hui comme dans le futur.
/// </para>
/// </remarks>
public static class EmailFormat
{
    private static readonly CultureInfo Francais = CultureInfo.GetCultureInfo("fr-FR");

    /// <summary>
    /// Rend une date avec son jour de la semaine et son heure.
    /// </summary>
    /// <param name="instant">Instant à rendre.</param>
    /// <returns>Date libellée en français.</returns>
    public static string Date(DateTime instant) =>
        string.Create(
            Francais,
            $"{instant:dddd d MMMM yyyy} à {instant:HH\\ h\\ mm}");

    /// <summary>
    /// Rend une date sans l'heure, pour un message qui n'en a pas besoin.
    /// </summary>
    /// <param name="instant">Instant à rendre.</param>
    /// <returns>Date libellée en français.</returns>
    public static string Jour(DateTime instant) =>
        string.Create(Francais, $"{instant:dddd d MMMM yyyy}");
}
