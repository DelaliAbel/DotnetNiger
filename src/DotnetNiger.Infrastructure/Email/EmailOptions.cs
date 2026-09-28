using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DotnetNiger.Infrastructure.Email;

/// <summary>
/// Paramètres du serveur d'envoi des courriels.
/// </summary>
public sealed class EmailOptions
{
    /// <summary>
    /// Nom de la section de configuration.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// Hôte du serveur SMTP.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Port du serveur SMTP.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    /// <summary>
    /// Utilisation d'une connexion chiffrée au niveau du transport.
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Identifiant de connexion. Absent pour un relais qui n'en demande pas.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Mot de passe de connexion. À fournir par un secret, jamais dans un fichier versionné.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Adresse d'expédition.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Nom affiché à l'expédition.
    /// </summary>
    public string FromDisplayName { get; set; } = "DotnetNiger";

    /// <summary>
    /// Adresse de réponse, si différente de l'expédition.
    /// </summary>
    public string? ReplyToAddress { get; set; }

    /// <summary>
    /// Adresse de la communauté, destinataire des messages système.
    /// </summary>
    public string ContactAddress { get; set; } = string.Empty;

    /// <summary>
    /// Adresse publique du site, utilisée pour construire les liens des courriels.
    /// </summary>
    public string SiteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Vrai si l'envoi peut être tenté.
    /// </summary>
    /// <remarks>
    /// Un développement local fonctionne sans serveur SMTP. Dans ce cas les liens sont
    /// consignés dans le journal au lieu d'être envoyés, ce qui permet de tester le
    /// parcours d'inscription sans dépendre d'un fournisseur de courriel.
    /// </remarks>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host)
        && !string.IsNullOrWhiteSpace(FromAddress)
        && !string.IsNullOrWhiteSpace(SiteUrl);

    /// <summary>
    /// Vrai si un serveur de courriel est renseigné alors que l'envoi reste impossible.
    /// </summary>
    /// <remarks>
    /// Ce cas est celui d'un déploiement où l'on a renseigné le serveur SMTP et oublié
    /// l'adresse du site ou l'expéditeur. Sans signal, l'application démarre normalement et
    /// se contente de consigner les messages au journal : les membres ne recevraient rien,
    /// et personne ne le remarquerait avant le premier signalement d'un membre. Le
    /// démarrage doit
    /// donc le signaler bruyamment.
    /// </remarks>
    public bool IsMisconfigured =>
        (!string.IsNullOrWhiteSpace(Host) || !string.IsNullOrWhiteSpace(FromAddress))
        && !IsConfigured;

    /// <summary>
    /// Liste les réglages manquants qui empêchent l'envoi.
    /// </summary>
    /// <returns>Noms des réglages absents, dans l'ordre où ils sont vérifiés.</returns>
    public IReadOnlyList<string> DescribeMissingSettings()
    {
        var manquants = new List<string>(3);

        if (string.IsNullOrWhiteSpace(Host))
        {
            manquants.Add($"{SectionName}:Host");
        }

        if (string.IsNullOrWhiteSpace(FromAddress))
        {
            manquants.Add($"{SectionName}:FromAddress");
        }

        if (string.IsNullOrWhiteSpace(SiteUrl))
        {
            manquants.Add($"{SectionName}:SiteUrl");
        }

        return manquants;
    }

    /// <summary>
    /// Construit une URL absolue à partir d'un chemin relatif de l'application.
    /// </summary>
    /// <param name="relativePath">Chemin relatif, commençant par <c>/</c>.</param>
    /// <returns>URL absolue.</returns>
    /// <exception cref="InvalidOperationException">L'adresse du site est absente.</exception>
    public string BuildUrl(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        if (string.IsNullOrWhiteSpace(SiteUrl))
        {
            throw new InvalidOperationException(
                "« Email:SiteUrl » est absent : impossible de construire un lien de courriel.");
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{SiteUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}");
    }
}
