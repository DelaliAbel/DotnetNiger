using DotnetNiger.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DotnetNiger.Web.Controller.Common;

/// <summary>
/// Journalisation de l'API.
/// </summary>
/// <remarks>
/// Les points de terminaison et le gestionnaire d'exceptions sont des méthodes statiques ou
/// des types sans dépendance au service de journalisation paramétré. Cette classe dédiée
/// reçoit les messages générés à la compilation, comme le fait
/// <c>Authentication/AccountLog.cs</c> pour le parcours de compte.
/// </remarks>
internal static partial class ApiLog
{
    [LoggerMessage(
        EventId = 70,
        Level = LogLevel.Information,
        Message = "{Requete} {Methode} répond {Statut} en {DureeMs} ms")]
    internal static partial void RequeteTraitee(
        ILogger logger,
        string requete,
        string methode,
        int statut,
        long dureeMs);

    [LoggerMessage(
        EventId = 71,
        Level = LogLevel.Warning,
        Message = "Règle métier refusée sur {Path} : {Code} — {Message}")]
    internal static partial void RegleMetierRefusee(
        ILogger logger,
        string path,
        string code,
        string message);

    [LoggerMessage(
        EventId = 72,
        Level = LogLevel.Information,
        Message = "Commentaire {CommentaireId} déposé par {Auteur}, état {Statut}")]
    internal static partial void CommentaireDepose(
        ILogger logger,
        Guid commentaireId,
        Guid auteur,
        CommentStatus statut);

    [LoggerMessage(
        EventId = 73,
        Level = LogLevel.Warning,
        Message = "Requête mal formée sur {Path} : {Message}")]
    internal static partial void RequeteMalFormee(
        ILogger logger,
        string path,
        string message);
}
