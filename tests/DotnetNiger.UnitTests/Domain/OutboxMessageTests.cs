using DotnetNiger.Domain.Common;
using DotnetNiger.Domain.Entities.Notifications;
using DotnetNiger.Domain.Enums;
using DotnetNiger.Domain.Exceptions;

namespace DotnetNiger.UnitTests.Domain;

/// <summary>
/// Vérifie la programmation des nouvelles tentatives d'un courriel en file.
/// </summary>
/// <remarks>
/// Ces règles décident si un courriel finit par partir ou disparaît. Une attente qui ne
/// croît pas martèle un serveur de courriel en panne ; une attente qui ne diminue jamais
/// laisse un message en attente pendant toute la journée.
/// </remarks>
public sealed class OutboxMessageTests
{
    private static readonly DateTime Depart = new(2026, 9, 27, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void UnMessageNeufEstDueImmédiatement()
    {
        var message = new OutboxMessage("test", "a@example.test", "Objet", "<p>Bonjour</p>", "Bonjour", Depart);

        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.True(message.IsDue(Depart));
    }

    [Fact]
    public void UneAttenteQuiCroîtÀChaqueÉchec()
    {
        var message = new OutboxMessage("test", "a@example.test", "Objet", "<p>Bonjour</p>", "Bonjour", Depart);

        Assert.True(message.ScheduleRetry("SMTP injoignable", 7, Depart));
        var premier = message.NextAttemptAtUtc;

        Assert.True(message.ScheduleRetry("SMTP injoignable", 7, Depart));
        var second = message.NextAttemptAtUtc;

        Assert.True(second > premier);
        Assert.Equal(2, message.Attempts);
    }

    [Fact]
    public void UneAttenteEstPlafonnée()
    {
        var message = new OutboxMessage("test", "a@example.test", "Objet", "<p>Bonjour</p>", "Bonjour", Depart);

        for (var essai = 0; essai < 30; essai++)
        {
            message.ScheduleRetry("SMTP injoignable", 100, Depart);
        }

        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(Depart.Add(OutboxMessage.MaxRetryDelay), message.NextAttemptAtUtc);
    }

    [Fact]
    public void UnMessageEstAbandonnéQuandLesTentativesSontÉpuisées()
    {
        var message = new OutboxMessage("test", "a@example.test", "Objet", "<p>Bonjour</p>", "Bonjour", Depart);

        Assert.True(message.ScheduleRetry("SMTP injoignable", 3, Depart));
        Assert.True(message.ScheduleRetry("SMTP injoignable", 3, Depart));
        Assert.False(message.ScheduleRetry("SMTP injoignable", 3, Depart));

        Assert.Equal(OutboxMessageStatus.Failed, message.Status);
        Assert.Null(message.NextAttemptAtUtc);
        Assert.Equal(3, message.Attempts);
    }

    [Fact]
    public void UnMessageAbandonnéNEstPlusDû()
    {
        var message = new OutboxMessage("test", "a@example.test", "Objet", "<p>Bonjour</p>", "Bonjour", Depart);

        message.ScheduleRetry("SMTP injoignable", 1, Depart);

        Assert.False(message.IsDue(Depart.AddDays(1)));
    }

    [Fact]
    public void UneExpéditionRéussieClôtLeMessage()
    {
        var message = new OutboxMessage("test", "a@example.test", "Objet", "<p>Bonjour</p>", "Bonjour", Depart);
        var envoye = Depart.AddMinutes(2);

        message.MarkSent(envoye);

        Assert.Equal(OutboxMessageStatus.Sent, message.Status);
        Assert.Equal(envoye, message.SentAtUtc);
        Assert.Null(message.NextAttemptAtUtc);
        Assert.Null(message.LastError);
    }

    [Fact]
    public void UnEnvoiRéussiEffaceLErreurPrécédente()
    {
        var message = new OutboxMessage("test", "a@example.test", "Objet", "<p>Bonjour</p>", "Bonjour", Depart);

        message.ScheduleRetry("SMTP injoignable", 7, Depart);
        message.MarkSent(Depart.AddMinutes(1));

        Assert.Null(message.LastError);
    }

    [Fact]
    public void UnMessageEnAttenteNEstPasDûAvantSonHeure()
    {
        var message = new OutboxMessage("test", "a@example.test", "Objet", "<p>Bonjour</p>", "Bonjour", Depart);

        message.ScheduleRetry("SMTP injoignable", 7, Depart);

        Assert.False(message.IsDue(Depart));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UnDestinataireVideEstRefusé(string destinataire)
    {
        var erreur = Assert.Throws<DomainException>(
            () => new OutboxMessage("test", destinataire, "Objet", "<p>Bonjour</p>", "Bonjour", Depart));

        Assert.Equal("field_required", erreur.Code);
    }

    [Fact]
    public void UnObjetTropLongEstRefusé()
    {
        var erreur = Assert.Throws<DomainException>(
            () => new OutboxMessage("test", "a@example.test", new string('x', FieldLimits.Subject + 1), "<p>Bonjour</p>", "Bonjour", Depart));

        Assert.Equal("field_too_long", erreur.Code);
    }
}
