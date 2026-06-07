using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotnetNiger.Community.Tests.Application.Services;

public class MemberDirectoryServiceTests
{
    private static AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMembers_WhenQueryEmpty()
    {
        var db = CreateDb();
        db.Members.AddRange(
            new Member { Id = Guid.NewGuid(), FullName = "Alice", Bio = "Bio A", Country = "FR", City = "Paris" },
            new Member { Id = Guid.NewGuid(), FullName = "Bob", Bio = "Bio B", Country = "US", City = "NYC" },
            new Member { Id = Guid.NewGuid(), FullName = "Charlie", Bio = "Bio C", Country = "FR", City = "Lyon" });
        await db.SaveChangesAsync();

        var svc = new MemberDirectoryService(db);
        var result = await svc.GetAllAsync(null, null, 1, 10);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByQuery()
    {
        var db = CreateDb();
        db.Members.AddRange(
            new Member { Id = Guid.NewGuid(), FullName = "Alpha", Bio = "Developer", Country = "FR", City = "Paris" },
            new Member { Id = Guid.NewGuid(), FullName = "Beta", Bio = "Designer", Country = "US", City = "NYC" });
        await db.SaveChangesAsync();

        var svc = new MemberDirectoryService(db);
        var result = await svc.GetAllAsync("Alpha", null, 1, 10);

        result.Items.Should().HaveCount(1);
        result.Items[0].FullName.Should().Be("Alpha");
    }

    [Fact]
    public async Task GetAllAsync_FiltersByCountry()
    {
        var db = CreateDb();
        db.Members.AddRange(
            new Member { Id = Guid.NewGuid(), FullName = "Alice", Country = "FR", City = "Paris" },
            new Member { Id = Guid.NewGuid(), FullName = "Bob", Country = "US", City = "NYC" },
            new Member { Id = Guid.NewGuid(), FullName = "Charlie", Country = "FR", City = "Lyon" });
        await db.SaveChangesAsync();

        var svc = new MemberDirectoryService(db);
        var result = await svc.GetAllAsync(null, "FR", 1, 10);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(m => m.Country == "FR");
    }

    [Fact]
    public async Task GetAllAsync_PaginationWorks()
    {
        var db = CreateDb();
        for (int i = 1; i <= 5; i++)
            db.Members.Add(new Member { Id = Guid.NewGuid(), FullName = $"Member {i}", Country = "FR", City = "Paris" });
        await db.SaveChangesAsync();

        var svc = new MemberDirectoryService(db);
        var page1 = await svc.GetAllAsync(null, null, 1, 2);
        var page2 = await svc.GetAllAsync(null, null, 2, 2);

        page1.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page2.Items.Should().HaveCount(2);
        page2.TotalCount.Should().Be(5);
    }
}
