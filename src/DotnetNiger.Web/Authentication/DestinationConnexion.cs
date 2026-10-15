using DotnetNiger.Application.Common;

namespace DotnetNiger.Web.Authentication;

/// <summary>
/// Destination d'un membre après sa connexion.
/// </summary>
/// <remarks>
/// La destination demandée reste prioritaire : quelqu'un qui visait un formulaire avant de
/// se connecter doit y arriver, quel que soit son rôle.
/// <para>
/// À défaut de destination, le rôle tranche : un administrateur entre par l'administration,
/// un membre par son tableau de bord. Sans cela, un administrateur atterrit devant une page
/// où il n'a rien à faire.
/// </para>
/// </remarks>
public static class DestinationConnexion
{
    /// <summary>Destination d'un administrateur.</summary>
    public const string Administration = "/administration";

    /// <summary>
    /// Choisit la destination d'un membre qui vient d'ouvrir une session.
    /// </summary>
    /// <param name="demande">Destination demandée avant la connexion, si elle existe.</param>
    /// <param name="estAdministrateur">Vrai si le compte porte le rôle administrateur.</param>
    /// <returns>L'adresse vers laquelle rediriger.</returns>
    public static string Pour(string? demande, bool estAdministrateur) =>
        ExternalSignInNavigation.IsLocal(demande)
            ? demande!
            : estAdministrateur
                ? Administration
                : ExternalSignInNavigation.EspaceMembre;
}