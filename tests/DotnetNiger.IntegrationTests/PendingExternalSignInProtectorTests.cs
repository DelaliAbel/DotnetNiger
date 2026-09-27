using System.Text;
using System.Text.Json;
using DotnetNiger.Web.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace DotnetNiger.IntegrationTests;

/// <summary>
/// Vérifie le transport chiffré de l'identité externe en attente de mot de passe.
/// </summary>
public class PendingExternalSignInProtectorTests
{
    // Clés en mémoire uniquement : chaque exécution de la suite repart de zéro, et aucun
    // fichier de clés n'est écrit sur le disque de la machine.
    private readonly IDataProtectionProvider _protection = new EphemeralDataProtectionProvider();

    private sealed class Horloge(DateTimeOffset instant) : TimeProvider
    {
        public DateTimeOffset Instant { get; set; } = instant;

        public override DateTimeOffset GetUtcNow() => Instant;
    }

    private static readonly PendingExternalSignIn Identite = new(
        "GitHub",
        "cle-privée-1234",
        "membre@example.org",
        EmailVerified: false,
        ReturnUrl: "/communaute");

    private (PendingExternalSignInProtector Protector, Horloge Clock) Create(DateTimeOffset instant)
    {
        var clock = new Horloge(instant);
        return (new PendingExternalSignInProtector(_protection, clock), clock);
    }

    [Fact]
    public void Aller_retour_preserve_l_identite()
    {
        var (protector, _) = Create(DateTimeOffset.Parse("2026-09-27T10:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture));

        var resultat = protector.Unprotect(protector.Protect(Identite));

        Assert.Equal(Identite, resultat);
    }

    [Fact]
    public void Le_jeton_ne_rivele_ni_l_adresse_ni_la_cle()
    {
        var (protector, _) = Create(DateTimeOffset.Parse("2026-09-27T10:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture));

        var jeton = protector.Protect(Identite);

        // La barre d'adresse ne doit rien laisser fuiter : ni l'adresse, ni l'identifiant
        // du compte chez le fournisseur.
        Assert.DoesNotContain("membre@example.org", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(jeton)));
        Assert.DoesNotContain("cle-privée-1234", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(jeton)));
    }

    [Fact]
    public void Un_jeton_perime_est_refuse()
    {
        var depart = DateTimeOffset.Parse("2026-09-27T10:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture);
        var (protector, clock) = Create(depart);

        var jeton = protector.Protect(Identite);

        // La protection de données ne limite pas la durée de vie du jeton : c'est
        // l'horodatage embarqué qui doit le rendre inutilisable.
        clock.Instant = depart.AddMinutes(16);

        Assert.Null(protector.Unprotect(jeton));
    }

    [Fact]
    public void Un_jeton_recu_avant_expiration_est_accepte()
    {
        var depart = DateTimeOffset.Parse("2026-09-27T10:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture);
        var (protector, clock) = Create(depart);

        var jeton = protector.Protect(Identite);

        clock.Instant = depart.AddMinutes(14);

        Assert.Equal(Identite, protector.Unprotect(jeton));
    }

    [Fact]
    public void Un_jeton_absent_vide_ou_abîmé_est_refuse()
    {
        var (protector, _) = Create(DateTimeOffset.Parse("2026-09-27T10:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture));

        Assert.Null(protector.Unprotect(null));
        Assert.Null(protector.Unprotect("   "));
        Assert.Null(protector.Unprotect("pas-un-jeton"));
        Assert.Null(protector.Unprotect(protector.Protect(Identite)[..^4] + "AAAA"));
    }

    [Fact]
    public void Un_jeton_fabrique_de_l_origine_est_refuse()
    {
        var (protector, _) = Create(DateTimeOffset.Parse("2026-09-27T10:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture));

        // Un attaquant ne peut pas fabriquer son propre jeton : il lui faudrait la clé de
        // protection, qui n'existe que sur le serveur.
        var faux = JsonSerializer.Serialize(new
        {
            Provider = "GitHub",
            ProviderKey = "cle-volee",
            Email = "victime@example.org",
            EmailVerified = true,
            ReturnUrl = "/",
        });

        var jeton = Convert.ToBase64String(Encoding.UTF8.GetBytes(faux));

        Assert.Null(protector.Unprotect(jeton));
    }
}
