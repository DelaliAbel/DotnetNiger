using Microsoft.Extensions.Logging;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Journalisation du parcours de compte.
/// </summary>
/// <remarks>
/// Les points de terminaison sont des méthodes statiques, qui ne peuvent pas servir de type
/// générique pour <see cref="ILogger{TCategoryName}"/>. Cette classe dédiée reçoit les
/// messages générés à la compilation.
/// </remarks>
internal static partial class AccountLog
{
    [LoggerMessage(
        EventId = 40,
        Level = LogLevel.Warning,
        Message = "Inscription refusée : {Motif}")]
    internal static partial void InscriptionRefusee(ILogger logger, string motif);

    [LoggerMessage(
        EventId = 41,
        Level = LogLevel.Information,
        Message = "Compte créé par inscription locale : {Email}")]
    internal static partial void CompteCree(ILogger logger, string email);

    [LoggerMessage(
        EventId = 42,
        Level = LogLevel.Warning,
        Message = "Le courriel de confirmation n'a pas pu être envoyé à {Email}.")]
    internal static partial void CourrielConfirmationEchoue(ILogger logger, string email);

    [LoggerMessage(
        EventId = 43,
        Level = LogLevel.Information,
        Message = "Le mot de passe d'un compte externe a été défini : {Email}")]
    internal static partial void MotDePasseExterneDefini(ILogger logger, string email);

    [LoggerMessage(
        EventId = 44,
        Level = LogLevel.Warning,
        Message = "Création d'un compte depuis un fournisseur externe refusée pour {Email}.")]
    internal static partial void CompteExterneRefuse(ILogger logger, string email);

    [LoggerMessage(
        EventId = 45,
        Level = LogLevel.Warning,
        Message = "Preuve de propriété refusée pour {Email}.")]
    internal static partial void PreuveRefusee(ILogger logger, string email);

    [LoggerMessage(
        EventId = 46,
        Level = LogLevel.Information,
        Message = "Lien de réinitialisation envoyé à {Email}.")]
    internal static partial void ReinitialisationEnvoyee(ILogger logger, string email);

    [LoggerMessage(
        EventId = 47,
        Level = LogLevel.Warning,
        Message = "Réinitialisation refusée pour {Email} : jeton invalide ou mot de passe non conforme.")]
    internal static partial void ReinitialisationRefusee(ILogger logger, string email);

    [LoggerMessage(
        EventId = 48,
        Level = LogLevel.Information,
        Message = "Mot de passe réinitialisé pour {Email}.")]
    internal static partial void MotDePasseReinitialise(ILogger logger, string email);

    [LoggerMessage(
        EventId = 49,
        Level = LogLevel.Warning,
        Message = "Le courriel de réinitialisation n'a pas pu être envoyé à {Email}.")]
    internal static partial void CourrielReinitialisationEchoue(ILogger logger, string email);

    [LoggerMessage(
        EventId = 50,
        Level = LogLevel.Information,
        Message = "Aucun lien de réinitialisation composé pour {Email} : aucun serveur de courriel n'est configuré")]
    internal static partial void CourrielReinitialisationNonConfigure(ILogger logger, string email);

    [LoggerMessage(
        EventId = 51,
        Level = LogLevel.Information,
        Message = "Demande de réinitialisation pour une adresse sans compte : {Email}")]
    internal static partial void ReinitialisationDemandeeInconnue(ILogger logger, string email);
}
