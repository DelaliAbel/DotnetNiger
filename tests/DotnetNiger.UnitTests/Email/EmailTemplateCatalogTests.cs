using DotnetNiger.Infrastructure.Email;

namespace DotnetNiger.UnitTests.Email;

/// <summary>
/// Vérifie que le catalogue des modèles de courriel tient ses promesses.
/// </summary>
/// <remarks>
/// Ces tests protègent l'administration autant que le code. Un modèle qui ne se rend plus,
/// une clé déclarée sans modèle, une variable oubliée : chacun de ces défauts se
/// accompagnerait d'un courriel raté en production, et d'un message que personne n'a vu
/// relire avant l'envoi.
/// </remarks>
public class EmailTemplateCatalogTests
{
    /// <summary>
    /// Toutes les clés déclarées par <see cref="EmailTemplateKeys"/>.
    /// </summary>
    /// <remarks>
    /// La liste est recopiée ici à la main, volontairement. Si le catalogue et les constantes
    /// en perdaient une entrée en même temps, la comparaison resterait satisfaite ; l'écart
    /// entre les deux listes est donc ce que ces tests cherchent à voir.
    /// </remarks>
    private static readonly string[] ClesAttendues =
    [
        EmailTemplateKeys.AccountConfirmation,
        EmailTemplateKeys.AccountWelcome,
        EmailTemplateKeys.AccountPasswordReset,
        EmailTemplateKeys.AccountDeletionConfirmation,
        EmailTemplateKeys.NewsletterConfirmation,
        EmailTemplateKeys.NewsletterUnsubscribed,
        EmailTemplateKeys.NewsletterNewContent,
        EmailTemplateKeys.EventRegistrationConfirmed,
        EmailTemplateKeys.EventRegistrationWaitlisted,
        EmailTemplateKeys.EventWaitlistPromoted,
        EmailTemplateKeys.EventCancelled,
        EmailTemplateKeys.EventUpdated,
        EmailTemplateKeys.CommentPosted,
        EmailTemplateKeys.ContentRepublished,
        EmailTemplateKeys.ContactAcknowledged,
        EmailTemplateKeys.ContactForwarded,
    ];

    [Fact]
    public void ChaqueCleDeclareePossedeUnModele()
    {
        var manquantes = ClesAttendues
            .Where(cle => !EmailTemplateCatalog.Essayer(cle, out _))
            .ToArray();

        Assert.Empty(manquantes);
    }

    [Fact]
    public void AucunModeleNUtiliseUneCleInconnue()
    {
        var declarees = new HashSet<string>(ClesAttendues, StringComparer.Ordinal);

        var inconnues = EmailTemplateCatalog.Tous
            .Select(modele => modele.Cle)
            .Where(cle => !declarees.Contains(cle))
            .ToArray();

        Assert.Empty(inconnues);
    }

    [Fact]
    public void AucunModeleNEstDeclaresDeuxFois()
    {
        var doublons = EmailTemplateCatalog.Tous
            .GroupBy(modele => modele.Cle, StringComparer.Ordinal)
            .Where(groupement => groupement.Count() > 1)
            .Select(groupement => groupement.Key)
            .ToArray();

        Assert.Empty(doublons);
    }

    /// <summary>
    /// Le test le plus utile du fichier : il rend chaque modèle avec ses propres valeurs
    /// d'exemple. Une variable renommée, retirée ou mal orthographiée échoue ici, au lieu
    /// d'échouer le jour où le modèle est envoyé à un membre.
    /// </summary>
    [Fact]
    public void TousLesModelesSeRendentAvecLeurExemple()
    {
        foreach (var modele in EmailTemplateCatalog.Tous)
        {
            var message = modele.RendreExemple(EmailBranding.Fictif);

            Assert.False(string.IsNullOrWhiteSpace(message.Subject));
            Assert.False(string.IsNullOrWhiteSpace(message.HtmlBody));
            Assert.False(string.IsNullOrWhiteSpace(message.TextBody));
        }
    }

    [Fact]
    public void ChaqueModeleDeclareToutesSesVariables()
    {
        foreach (var modele in EmailTemplateCatalog.Tous)
        {
            foreach (var variable in modele.Variables)
            {
                Assert.True(
                    modele.Exemple.Contient(variable),
                    $"Le modèle « {modele.Cle} » déclare la variable {variable}, absente de son exemple.");
            }
        }
    }

    [Fact]
    public void ChaqueModeleALaMemeVersionHtmlEtTexte()
    {
        foreach (var modele in EmailTemplateCatalog.Tous)
        {
            var message = modele.RendreExemple(EmailBranding.Fictif);

            // Un contenu présent dans un corps et absent de l'autre trahit un oubli : le
            // membre lisant en texte brut ne doit pas perdre une information.
            foreach (var paragraphe in modele.Contenu(modele.Exemple).Paragraphes)
            {
                var fragment = FragmentSigne(paragraphe);

                if (fragment.Length > 8)
                {
                    Assert.Contains(
                        fragment,
                        message.TextBody,
                        StringComparison.Ordinal);
                }
            }
        }
    }

    [Fact]
    public void LaLettreDInformationProposeUnDesabonnement()
    {
        // Seule la lettre d'information est une diffusion : c'est elle qui arrive en
        // masse, sans demande préalable, et qui doit donc porter le lien de désinscription.
        // Les autres messages partent en réponse à une action du membre — confirmation de
        // son inscription, réponse à son contact, changement de mot de passe — et n'en ont
        // pas besoin, pas plus que la confirmation d'abonnement elle-même.
        var modele = EmailTemplateCatalog.Obtenir(EmailTemplateKeys.NewsletterNewContent);

        Assert.NotNull(modele.Contenu(modele.Exemple).UrlDesinscription);

        var message = modele.RendreExemple(EmailBranding.Fictif);

        Assert.Contains("Se désinscrire", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains(
            modele.Contenu(modele.Exemple).UrlDesinscription!,
            message.TextBody,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ChaqueModelePorteUnMotifDeReception()
    {
        // Motif obligatoire : sans lui, le message est indéfendable devant un filtre.
        foreach (var modele in EmailTemplateCatalog.Tous)
        {
            Assert.False(string.IsNullOrWhiteSpace(modele.Raison));
        }
    }

    [Fact]
    public void UneCleInconnueEstRefusee()
    {
        Assert.False(EmailTemplateCatalog.Essayer("inconnu.modele", out _));
        Assert.Throws<InvalidOperationException>(
            () => EmailTemplateCatalog.Obtenir("inconnu.modele"));
    }

    [Fact]
    public void LeBandeauReferenceLeLogoEtLeNomDuSite()
    {
        var message = EmailTemplateCatalog
            .Obtenir(EmailTemplateKeys.AccountWelcome)
            .RendreExemple(EmailBranding.Fictif);

        // Le nom en toutes lettres accompagne le logo : si l'image est bloquée, le message
        // doit rester identifiable.
        Assert.Contains(EmailBranding.SiteName, message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains($"{EmailBranding.Fictif.SiteUrl}/favicon.png", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains(EmailBranding.Tagline, message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public void LObjetNePeutPasInjecterUnEnTete()
    {
        // L'objet est le seul texte a finir dans un en-tête de courriel. Un retour a la ligne
        // issu d'une valeur pourrait y ajouter un en-tête falsifié.
        var modele = EmailTemplateCatalog.Obtenir(EmailTemplateKeys.EventRegistrationConfirmed);

        var message = modele.Rendre(
            EmailValues.De(
                (EmailVariables.NomMembre, "Awa"),
                (EmailVariables.TitreEvenement, "Rencontre\r\nBcc: attaquant@exemple.test"),
                (EmailVariables.DateEvenement, "samedi 18 mars 2026"),
                (EmailVariables.LieuEvenement, "Niamey"),
                (EmailVariables.UrlEvenement, "https://dotnetniger.test/e")),
            EmailBranding.Fictif);

        Assert.DoesNotContain('\n', message.Subject);
        Assert.DoesNotContain('\r', message.Subject);
    }

    [Fact]
    public void UnLienNonHttpEstRefuse()
    {
        // Un schema javascript: dans un href transformerait le bouton en piege.
        var modele = EmailTemplateCatalog.Obtenir(EmailTemplateKeys.AccountWelcome);

        Assert.Throws<InvalidOperationException>(() => modele.Rendre(
            EmailValues.De(
                (EmailVariables.NomMembre, "Awa"),
                (EmailVariables.UrlCompte, "javascript:alert(1)")),
            EmailBranding.Fictif));
    }

    [Fact]
    public void UneVariableAbsenteEstSignalee()
    {
        var modele = EmailTemplateCatalog.Obtenir(EmailTemplateKeys.AccountWelcome);

        Assert.Throws<InvalidOperationException>(
            () => modele.Rendre(EmailValues.De((EmailVariables.NomMembre, "Awa")), EmailBranding.Fictif));
    }

    [Fact]
    public void UnNomAvecBaliseResteDuTexte()
    {
        // Un membre qui s'appelle <b>Awa</b> ne doit pas pouvoir injecter du style dans
        // le message envoye aux autres.
        var modele = EmailTemplateCatalog.Obtenir(EmailTemplateKeys.AccountWelcome);

        var message = modele.Rendre(
            EmailValues.De(
                (EmailVariables.NomMembre, "<b>Awa</b>"),
                (EmailVariables.UrlCompte, "https://dotnetniger.test/compte")),
            EmailBranding.Fictif);

        Assert.DoesNotContain("<b>Awa</b>", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;Awa&lt;/b&gt;", message.HtmlBody, StringComparison.Ordinal);

        // La version texte, elle, n'echappe rien : c'est du texte, pas du HTML.
        Assert.Contains("<b>Awa</b>", message.TextBody, StringComparison.Ordinal);
    }

    /// <summary>
    /// Extrait d'un paragraphe la partie texte, sans balise, pour la comparer au corps texte.
    /// </summary>
    /// <param name="paragraphe">Paragraphe produit par un modèle.</param>
    /// <returns>Fragment comparable.</returns>
    private static string FragmentSigne(string paragraphe)
    {
        var debut = paragraphe.IndexOf('>');
        var fin = paragraphe.LastIndexOf('>');

        return debut >= 0 && fin > debut
            ? paragraphe[(debut + 1)..fin].Trim()
            : paragraphe.Trim();
    }
}
