namespace DotnetNiger.Infrastructure.Identity;

/// <summary>
/// Bilan de la suppression définitive d'un compte.
/// </summary>
/// <param name="Comments">Commentaires redactés par le compte, réponses comprises.</param>
/// <param name="Articles">Articles dont le compte était l'auteur.</param>
/// <param name="Resources">Ressources dont le compte était le créateur.</param>
/// <param name="Projects">Projets lancés par le compte.</param>
/// <param name="Events">Événements créés par le compte.</param>
/// <param name="Registrations">Inscriptions à des événements dont le compte était membre.</param>
/// <param name="Reports">Signalements déposés par le compte.</param>
/// <param name="ReleasedReports">Signalements traités par le compte, rendus à la modération.</param>
public sealed record AccountDeletionReport(
    int Comments,
    int Articles,
    int Resources,
    int Projects,
    int Events,
    int Registrations,
    int Reports,
    int ReleasedReports)
{
    /// <summary>Nombre total d'écritures retirées de la base.</summary>
    public int Total =>
        Comments + Articles + Resources + Projects + Events + Registrations + Reports;
}

/// <summary>
/// Suppression définitive d'un compte et de ce qui lui appartient.
/// </summary>
public interface IAccountDeletionService
{
    /// <summary>
    /// Efface définitivement un compte et les contenus dont il est l'auteur.
    /// </summary>
    /// <param name="accountId">Identifiant du compte à supprimer.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>
    /// Le bilan des retraits, ou <see langword="null"/> si le compte n'existe pas.
    /// </returns>
    /// <remarks>
    /// L'opération est irréversible et n'a pas d'inverse : c'est le comportement demandé
    /// pour une suppression de compte, mais elle ne peut pas être reconstruite depuis une
    /// sauvegarde absente.
    /// </remarks>
    Task<AccountDeletionReport?> DeleteAsync(Guid accountId, CancellationToken cancellationToken = default);
}
