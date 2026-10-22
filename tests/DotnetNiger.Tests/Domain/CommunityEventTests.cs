using DotnetNiger.Domain.Entities.Community;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie les créneaux, la cohérence format/lieu/lien, et l'ouverture des inscriptions.
/// </summary>
public sealed class CommunityEventTests
{
    private static readonly Guid Admin = Guid.NewGuid();
    private static readonly DateTime Debut = new(2026, 6, 1, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Fin = Debut.AddHours(2);

    private static CommunityEvent Creer(
        CommunityEventFormat format = CommunityEventFormat.InPerson,
        string titre = "Meetup Blazor",
        string? slug = null) =>
        new(Admin, titre, "Résumé", "<p>Description</p>", format, Debut, Fin, slug);

    [Fact]
    public void Constructeur_DeriveLeSlugDuTitre()
    {
        var evenement = Creer(titre: "Soirée Blazor à Abidjan");

        Assert.Equal("soiree-blazor-a-abidjan", evenement.Slug);
    }

    [Fact]
    public void Constructeur_UtiliseLeSlugFourni_QuandPresent()
    {
        var evenement = Creer(titre: "Titre quelconque", slug: "slug-impose");

        Assert.Equal("slug-impose", evenement.Slug);
    }

    [Fact]
    public void Constructeur_DemarreEnBrouillon()
    {
        var evenement = Creer();

        Assert.Equal(PublicationStatus.Draft, evenement.Status);
        Assert.Null(evenement.PublishedAtUtc);
    }

    [Fact]
    public void Constructeur_Throws_QuandFinAvantDebut()
    {
        var erreur = Assert.Throws<DomainException>(
            () => new CommunityEvent(Admin, "T", "S", "D", CommunityEventFormat.InPerson, Debut, Debut));

        Assert.Equal("event_invalid_schedule", erreur.Code);
    }

    [Fact]
    public void SetKind_TrimsAndStores()
    {
        var evenement = Creer();

        evenement.SetKind("  Meetup  ");

        Assert.Equal("Meetup", evenement.Kind);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void SetKind_Throws_WhenEmpty(string? kind)
    {
        var evenement = Creer();

        var erreur = Assert.Throws<DomainException>(() => evenement.SetKind(kind!));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void SetCoverImage_AcceptsSitePath()
    {
        var evenement = Creer();

        evenement.SetCoverImage("/api/images/affiche.webp");

        Assert.Equal("/api/images/affiche.webp", evenement.CoverImageUrl);
    }

    [Fact]
    public void SetCoverImage_Throws_WhenDangerousScheme()
    {
        var evenement = Creer();

        var erreur = Assert.Throws<DomainException>(
            () => evenement.SetCoverImage("javascript:alert(1)"));

        Assert.Equal("field_invalid_url", erreur.Code);
    }

    [Fact]
    public void Publish_InPerson_RequiertUnLieu()
    {
        var evenement = Creer(CommunityEventFormat.InPerson);

        var erreur = Assert.Throws<DomainException>(() => evenement.Publish(Debut));

        Assert.Equal("event_location_required", erreur.Code);
    }

    [Fact]
    public void Publish_InPerson_ReussitAvecUnLieu()
    {
        var evenement = Creer(CommunityEventFormat.InPerson);
        evenement.SetLocation("Abidjan, Cocody");

        evenement.Publish(Debut);

        Assert.Equal(PublicationStatus.Published, evenement.Status);
        Assert.Equal(Debut, evenement.PublishedAtUtc);
    }

    [Fact]
    public void Publish_Online_RequiertUnLien()
    {
        var evenement = Creer(CommunityEventFormat.Online);

        var erreur = Assert.Throws<DomainException>(() => evenement.Publish(Debut));

        Assert.Equal("event_online_url_required", erreur.Code);
    }

    [Fact]
    public void Publish_Hybrid_RequiertLieuEtLien()
    {
        var evenement = Creer(CommunityEventFormat.Hybrid);
        evenement.SetOnlineUrl("https://visio.exemple.test/salle");

        var erreur = Assert.Throws<DomainException>(() => evenement.Publish(Debut));
        Assert.Equal("event_location_required", erreur.Code);

        evenement.SetLocation("Abidjan");
        evenement.Publish(Debut);
        Assert.Equal(PublicationStatus.Published, evenement.Status);
    }

    [Fact]
    public void Publish_NeRemplacePasLaDateDePublication()
    {
        var evenement = Creer();
        evenement.SetLocation("Abidjan");
        evenement.Publish(Debut);

        evenement.Publish(Debut.AddDays(1));

        Assert.Equal(Debut, evenement.PublishedAtUtc);
    }

    [Fact]
    public void SetOnlineUrl_RejetteUrlRelative()
    {
        var evenement = Creer(CommunityEventFormat.Online);

        var erreur = Assert.Throws<DomainException>(
            () => evenement.SetOnlineUrl("/salle-en-ligne"));

        Assert.Equal("field_invalid_url", erreur.Code);
    }

    [Fact]
    public void SetCapacity_AccepteNullEtPositif_RejetteNegatif()
    {
        var evenement = Creer();

        evenement.SetCapacity(null);
        Assert.Null(evenement.Capacity);

        evenement.SetCapacity(42);
        Assert.Equal(42, evenement.Capacity);

        var erreur = Assert.Throws<DomainException>(() => evenement.SetCapacity(-1));
        Assert.Equal("field_negative", erreur.Code);
    }

    [Fact]
    public void UpdateDetails_ChangeLeSlug_QuandLeTitreChange()
    {
        var evenement = Creer(titre: "Ancien titre");

        evenement.UpdateDetails("Nouveau titre", "S", "D", Debut, Fin);

        Assert.Equal("nouveau-titre", evenement.Slug);
    }

    [Fact]
    public void UpdateDetails_ConserveLeSlug_QuandLeTitreEstInchange()
    {
        var evenement = Creer(titre: "Titre stable", slug: "slug-diffuse");

        evenement.UpdateDetails("Titre stable", "Autre résumé", "D", Debut, Fin);

        Assert.Equal("slug-diffuse", evenement.Slug);
    }

    [Fact]
    public void UpdateDetails_RejetteUnCreneauInvalide()
    {
        var evenement = Creer();

        var erreur = Assert.Throws<DomainException>(
            () => evenement.UpdateDetails("T", "S", "D", Fin, Debut));

        Assert.Equal("event_invalid_schedule", erreur.Code);
    }

    [Fact]
    public void CanRegister_RefuseTantQueNonPublie()
    {
        var evenement = Creer();
        evenement.SetLocation("Abidjan");

        Assert.False(evenement.CanRegister(0, Debut.AddDays(-1)));
    }

    [Fact]
    public void CanRegister_AccepteQuandPublieEtOuvert()
    {
        var evenement = Creer();
        evenement.SetLocation("Abidjan");
        evenement.Publish(Debut.AddDays(-10));

        Assert.True(evenement.CanRegister(0, Debut.AddDays(-1)));
    }

    [Fact]
    public void CanRegister_RefuseApresLeDebut()
    {
        var evenement = Creer();
        evenement.SetLocation("Abidjan");
        evenement.Publish(Debut.AddDays(-10));

        Assert.False(evenement.CanRegister(0, Debut));
    }

    [Fact]
    public void CanRegister_RefuseApresLaCloture()
    {
        var evenement = Creer();
        evenement.SetLocation("Abidjan");
        evenement.Publish(Debut.AddDays(-10));
        evenement.SetRegistrationClosesAt(Debut.AddDays(-2));

        Assert.False(evenement.CanRegister(0, Debut.AddDays(-1)));
    }

    [Fact]
    public void CanRegister_RefuseQuandComplet()
    {
        var evenement = Creer();
        evenement.SetLocation("Abidjan");
        evenement.Publish(Debut.AddDays(-10));
        evenement.SetCapacity(10);

        Assert.True(evenement.CanRegister(9, Debut.AddDays(-1)));
        Assert.False(evenement.CanRegister(10, Debut.AddDays(-1)));
        Assert.True(evenement.IsFull(10));
    }

    [Fact]
    public void IsFull_EstFauxSansLimite()
    {
        var evenement = Creer();

        Assert.False(evenement.IsFull(1_000_000));
    }

    [Fact]
    public void ReturnToDraftEtArchive_ChangentLEtat()
    {
        var evenement = Creer();

        evenement.ReturnToDraft();
        Assert.Equal(PublicationStatus.Draft, evenement.Status);

        evenement.Archive();
        Assert.Equal(PublicationStatus.Archived, evenement.Status);
    }
}
