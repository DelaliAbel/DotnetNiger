using Microsoft.Extensions.Logging;

namespace DotnetNiger.Web;

/// <summary>
/// Journalisation du démarrage de l'application.
/// </summary>
/// <remarks>
/// Le démarrage s'exprime en instructions de niveau supérieur, qui ne peuvent pas porter
/// une méthode <c>partial</c> locale. Cette classe dédiée reçoit les messages générés à la
/// compilation, comme le fait <c>Controller/Common/ApiLog.cs</c> pour les points de
/// terminaison.
/// </remarks>
internal static partial class StartupLog
{
    /// <summary>
    /// Signale une configuration de courriel incomplète qui empêche tout envoi.
    /// </summary>
    /// <param name="logger">Journal.</param>
    /// <param name="reglages">Réglages absents, séparés par des virgules.</param>
    [LoggerMessage(
        EventId = 90,
        Level = LogLevel.Error,
        Message = "Envoi des courriels désactivé : {Reglages} manquant(s). Les messages seront consignés au journal au lieu d'être expédiés.")]
    internal static partial void CourrielsDesactives(ILogger logger, string reglages);

    /// <summary>
    /// Confirme que l'envoi des courriels est opérationnel.
    /// </summary>
    /// <param name="logger">Journal.</param>
    [LoggerMessage(
        EventId = 91,
        Level = LogLevel.Information,
        Message = "Envoi des courriels opérationnel")]
    internal static partial void CourrielsActifs(ILogger logger);
}
