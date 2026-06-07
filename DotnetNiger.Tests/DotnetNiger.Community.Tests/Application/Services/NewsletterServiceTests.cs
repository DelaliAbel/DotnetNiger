using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotnetNiger.Community.Tests.Application.Services;

public class NewsletterServiceTests
{
    private static AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task SubscribeAsync_AddsSubscriber()
    {
        var db = CreateDb();
        var svc = new NewsletterService(db);
        var request = new SubscribeRequest("test@example.com", "Test User");

        var result = await svc.SubscribeAsync(request);

        result.Email.Should().Be("test@example.com");
        result.Name.Should().Be("Test User");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SubscribeAsync_DuplicateEmail_Throws()
    {
        var db = CreateDb();
        var svc = new NewsletterService(db);
        var request = new SubscribeRequest("test@example.com", "Test User");

        await svc.SubscribeAsync(request);
        var act = () => svc.SubscribeAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cet email est déjà abonné");
    }

    [Fact]
    public async Task UnsubscribeAsync_MarksAsInactive()
    {
        var db = CreateDb();
        var sub = new NewsletterSubscription
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            IsActive = true,
            UnsubscribeToken = "known-token",
            SubscribedAt = DateTime.UtcNow
        };
        db.NewsletterSubscriptions.Add(sub);
        await db.SaveChangesAsync();

        var svc = new NewsletterService(db);
        var result = await svc.UnsubscribeAsync(new UnsubscribeRequest("test@example.com", "known-token"));

        result.Should().BeTrue();
        var reloaded = await db.NewsletterSubscriptions.FirstAsync(s => s.Email == "test@example.com");
        reloaded.IsActive.Should().BeFalse();
        reloaded.UnsubscribedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveCountAsync_ReturnsCorrectCount()
    {
        var db = CreateDb();
        db.NewsletterSubscriptions.AddRange(
            new NewsletterSubscription { Id = Guid.NewGuid(), Email = "a@test.com", Name = "A", IsActive = true },
            new NewsletterSubscription { Id = Guid.NewGuid(), Email = "b@test.com", Name = "B", IsActive = true },
            new NewsletterSubscription { Id = Guid.NewGuid(), Email = "c@test.com", Name = "C", IsActive = false });
        await db.SaveChangesAsync();

        var svc = new NewsletterService(db);
        var count = await svc.GetActiveCountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_PaginationWorks()
    {
        var db = CreateDb();
        for (int i = 1; i <= 5; i++)
            db.NewsletterSubscriptions.Add(new NewsletterSubscription
            {
                Id = Guid.NewGuid(),
                Email = $"{i}@test.com",
                Name = $"User {i}",
                IsActive = true
            });
        await db.SaveChangesAsync();

        var svc = new NewsletterService(db);
        var page1 = await svc.GetAllAsync(1, 2);
        var page2 = await svc.GetAllAsync(2, 2);

        page1.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page2.Items.Should().HaveCount(2);
        page2.TotalCount.Should().Be(5);
    }
}
