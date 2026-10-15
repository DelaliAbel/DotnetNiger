using Microsoft.Extensions.Logging;

namespace DotnetNiger.Web.Services.Inscriptions;

/// <summary>
/// Journalisation des inscriptions aux événements.
/// </summary>
/// <remarks>
/// Les points de terminaison sont des méthodes statiques ou des types sans dépendance
/// au service de journalisation paramétré. Cette classe dédiée reçoit les messages
/// générés à la compilation, comme le fait <c>Controller/Common/ApiLog.cs</c>.
/// </remarks>
internal static partial class InscriptionLog
{
    [LoggerMessage(
        EventId = 161,
        Level = LogLevel.Information,
        Message = "Inscription de {MembreId} à l'événement {EvenementId} ({Slug})")]
    internal static partial void InscriptionEnregistree(
        ILogger logger,
        Guid membreId,
        Guid evenementId,
        string slug);

    [LoggerMessage(
        EventId = 162,
        Level = LogLevel.Information,
        Message = "Place libérée par {MembreId} sur l'événement {EvenementId} ({Slug})")]
    internal static partial void PlaceLiberee(
        ILogger logger,
        Guid membreId,
        Guid evenementId,
        string slug);
}
