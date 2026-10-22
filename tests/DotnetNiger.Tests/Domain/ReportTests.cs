using DotnetNiger.Domain.Entities.Moderation;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.Tests.Domain;

/// <summary>
/// Vérifie le cycle de vie d'un signalement : ouvert, en cours, puis clôturé une seule fois.
/// </summary>
public sealed class ReportTests
{
    private static readonly Guid Rapporteur = Guid.NewGuid();
    private static readonly Guid Moderateur = Guid.NewGuid();
    private static readonly DateTime Instant = new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Report Creer(string? details = null) =>
        new(ReportTargetType.Comment, Guid.NewGuid(), ReportReason.Spam, Rapporteur, details);

    [Fact]
    public void Constructeur_DemarreOuvert()
    {
        var signalement = Creer();

        Assert.Equal(ReportStatus.Open, signalement.Status);
        Assert.Equal(Rapporteur, signalement.ReporterId);
        Assert.Null(signalement.HandledById);
        Assert.Null(signalement.HandledAtUtc);
    }

    [Fact]
    public void Constructeur_Throws_QuandCibleVide()
    {
        var erreur = Assert.Throws<DomainException>(
            () => new Report(ReportTargetType.Comment, Guid.Empty, ReportReason.Spam, Rapporteur));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void Constructeur_Throws_QuandRapporteurVide()
    {
        var erreur = Assert.Throws<DomainException>(
            () => new Report(ReportTargetType.Comment, Guid.NewGuid(), ReportReason.Spam, Guid.Empty));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void Constructeur_Throws_QuandDetailsTropLongs()
    {
        var erreur = Assert.Throws<DomainException>(() => Creer(new string('a', 4_001)));

        Assert.Equal("field_too_long", erreur.Code);
    }

    [Fact]
    public void Constructeur_IgnoreDesDetailsBlancs()
    {
        Assert.Null(Creer("   ").Details);
    }

    [Fact]
    public void StartReview_PlaceEnCoursEtAffecte()
    {
        var signalement = Creer();

        signalement.StartReview(Moderateur);

        Assert.Equal(ReportStatus.UnderReview, signalement.Status);
        Assert.Equal(Moderateur, signalement.HandledById);
    }

    [Fact]
    public void StartReview_Throws_QuandModerateurVide()
    {
        var signalement = Creer();

        var erreur = Assert.Throws<DomainException>(() => signalement.StartReview(Guid.Empty));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void Resolve_ClotureEtConserveLaNote()
    {
        var signalement = Creer();
        signalement.StartReview(Moderateur);

        signalement.Resolve(Moderateur, "Commentaire masqué", Instant);

        Assert.Equal(ReportStatus.Resolved, signalement.Status);
        Assert.Equal("Commentaire masqué", signalement.ResolutionNote);
        Assert.Equal(Instant, signalement.HandledAtUtc);
    }

    [Fact]
    public void Dismiss_ClotureEnInfondé()
    {
        var signalement = Creer();

        signalement.Dismiss(Moderateur, null, Instant);

        Assert.Equal(ReportStatus.Dismissed, signalement.Status);
        Assert.Null(signalement.ResolutionNote);
    }

    [Fact]
    public void CloturerDeuxFois_EstRefuse()
    {
        var signalement = Creer();
        signalement.Resolve(Moderateur, "ok", Instant);

        var erreur = Assert.Throws<DomainException>(
            () => signalement.Dismiss(Moderateur, "non", Instant));

        Assert.Equal("report_already_closed", erreur.Code);
    }

    [Fact]
    public void StartReview_RefuseApresCloture()
    {
        var signalement = Creer();
        signalement.Resolve(Moderateur, "ok", Instant);

        var erreur = Assert.Throws<DomainException>(() => signalement.StartReview(Moderateur));

        Assert.Equal("report_already_closed", erreur.Code);
    }

    [Fact]
    public void ReleaseHandler_RetireLeGestionnaireSansChangerLEtat()
    {
        var signalement = Creer();
        signalement.Resolve(Moderateur, "décision conservée", Instant);

        signalement.ReleaseHandler();

        Assert.Null(signalement.HandledById);
        Assert.Equal(ReportStatus.Resolved, signalement.Status);
        Assert.Equal("décision conservée", signalement.ResolutionNote);
        Assert.Equal(Instant, signalement.HandledAtUtc);
    }
}
