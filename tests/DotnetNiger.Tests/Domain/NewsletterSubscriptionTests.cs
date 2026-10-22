using DotnetNiger.Domain.Entities.Content;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie le double opt-in : un abonnement ne devient actif qu'après confirmation, et un
/// jeton ne peut pas être rejoué pour rouvrir un abonnement refermé.
/// </summary>
public sealed class NewsletterSubscriptionTests
{
    private static readonly DateTime Demande = new(2026, 4, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Confirmation = new(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc);

    private static NewsletterSubscription Creer(
        string empreinte = "empreinte-1",
        string cle = "cle-1") =>
        NewsletterSubscription.Create("lecteur@exemple.test", empreinte, cle, Demande);

    [Fact]
    public void Create_InitialiseEnAttente()
    {
        var abonnement = Creer();

        Assert.Equal(NewsletterStatus.Pending, abonnement.Status);
        Assert.Equal("lecteur@exemple.test", abonnement.Email);
        Assert.False(abonnement.IsConfirmed);
        Assert.Null(abonnement.ConfirmedAtUtc);
    }

    [Fact]
    public void Create_Throws_QuandEmailVide()
    {
        var erreur = Assert.Throws<DomainException>(
            () => NewsletterSubscription.Create("  ", "e", "c", Demande));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void Create_Throws_QuandEmpreinteVide()
    {
        var erreur = Assert.Throws<DomainException>(
            () => NewsletterSubscription.Create("a@b.test", " ", "c", Demande));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void Confirm_AvecLeBonJeton_RendAbonne()
    {
        var abonnement = Creer();

        var confirme = abonnement.Confirm("empreinte-1", Confirmation);

        Assert.True(confirme);
        Assert.True(abonnement.IsConfirmed);
        Assert.Equal(Confirmation, abonnement.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_AvecUnMauvaisJeton_EstRefuse()
    {
        var abonnement = Creer();

        var confirme = abonnement.Confirm("mauvais-jeton", Confirmation);

        Assert.False(confirme);
        Assert.Equal(NewsletterStatus.Pending, abonnement.Status);
        Assert.Null(abonnement.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_DeuxFois_NEcrasePasLaPremiereDate()
    {
        var abonnement = Creer();
        abonnement.Confirm("empreinte-1", Confirmation);

        var seconde = abonnement.Confirm("empreinte-1", Confirmation.AddDays(3));

        Assert.False(seconde);
        Assert.Equal(Confirmation, abonnement.ConfirmedAtUtc);
    }

    [Fact]
    public void Unsubscribe_AvecLaBonneCle_RomptLAbonnement()
    {
        var abonnement = Creer();
        abonnement.Confirm("empreinte-1", Confirmation);

        var desabonne = abonnement.Unsubscribe("cle-1", Confirmation.AddDays(1));

        Assert.True(desabonne);
        Assert.Equal(NewsletterStatus.Unsubscribed, abonnement.Status);
        Assert.Equal(Confirmation.AddDays(1), abonnement.UnsubscribedAtUtc);
        Assert.False(abonnement.IsConfirmed);
    }

    [Fact]
    public void Unsubscribe_AvecUneMauvaiseCle_EstRefuse()
    {
        var abonnement = Creer();

        Assert.False(abonnement.Unsubscribe("mauvaise", Confirmation));
        Assert.Equal(NewsletterStatus.Pending, abonnement.Status);
    }

    [Fact]
    public void RequestAgain_RemetEnAttenteEtChangeLeJeton()
    {
        var abonnement = Creer();
        abonnement.Confirm("empreinte-1", Confirmation);

        abonnement.RequestAgain("empreinte-2", Demande.AddDays(5));

        Assert.Equal(NewsletterStatus.Pending, abonnement.Status);
        Assert.Equal("empreinte-2", abonnement.TokenHash);
        Assert.Equal(Demande.AddDays(5), abonnement.RequestedAtUtc);
        // L'ancien jeton ne doit plus rien confirmer.
        Assert.False(abonnement.Confirm("empreinte-1", Confirmation));
        Assert.True(abonnement.Confirm("empreinte-2", Confirmation));
    }

    [Fact]
    public void Confirm_ApresDesabonnement_ReprendLAbonnement()
    {
        var abonnement = Creer();
        abonnement.Confirm("empreinte-1", Confirmation);
        abonnement.Unsubscribe("cle-1", Confirmation.AddDays(1));

        var reconfirme = abonnement.Confirm("empreinte-1", Confirmation.AddDays(2));

        Assert.True(reconfirme);
        Assert.True(abonnement.IsConfirmed);
        Assert.Null(abonnement.UnsubscribedAtUtc);
        // La date d'origine est conservée : c'est l'ancienneté de l'abonné.
        Assert.Equal(Confirmation, abonnement.ConfirmedAtUtc);
    }
}
