using DotnetNiger.Community.Application.DTOs;
using DotnetNiger.Community.Application.Services;
using DotnetNiger.Community.Domain.Entities;
using DotnetNiger.Community.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DotnetNiger.Community.Tests.Application.Services;

public class PartnerServiceTests
{
    private static AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsActivePartners()
    {
        var db = CreateDb();
        db.Partners.AddRange(
            new Partner { Id = Guid.NewGuid(), Name = "Partner A", IsActive = true },
            new Partner { Id = Guid.NewGuid(), Name = "Partner B", IsActive = true },
            new Partner { Id = Guid.NewGuid(), Name = "Partner C", IsActive = false });
        await db.SaveChangesAsync();

        var svc = new PartnerService(db);
        var result = await svc.GetAllActiveAsync(null);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPartner()
    {
        var db = CreateDb();
        var id = Guid.NewGuid();
        db.Partners.Add(new Partner { Id = id, Name = "Partner A", IsActive = true });
        await db.SaveChangesAsync();

        var svc = new PartnerService(db);
        var result = await svc.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Partner A");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var db = CreateDb();
        var svc = new PartnerService(db);
        var result = await svc.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
