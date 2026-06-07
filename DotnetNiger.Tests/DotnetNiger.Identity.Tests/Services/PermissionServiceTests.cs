using Microsoft.EntityFrameworkCore;
using DotnetNiger.Identity.Application.DTOs;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using FluentAssertions;
using Xunit;

#pragma warning disable CS8604

namespace DotnetNiger.Identity.Tests.Services;

public class PermissionServiceTests
{
    private readonly IdentityDbContext _db;

    public PermissionServiceTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new IdentityDbContext(options, new TenantContext());
    }

    private PermissionService CreateSut() => new PermissionService(_db);

    [Fact]
    public async Task CreateAsync_Success()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        var request = new CreatePermissionRequest("users.read", "Users", tenantId);

        var result = await sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("users.read");
        result.Category.Should().Be("Users");
        result.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPermissions()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        var anotherTenantId = Guid.NewGuid();

        _db.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            Name = "users.read",
            Category = "Users",
            TenantId = tenantId
        });
        _db.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            Name = "users.write",
            Category = "Users",
            TenantId = tenantId
        });
        _db.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            Name = "reports.read",
            Category = "Reports",
            TenantId = tenantId
        });
        _db.Permissions.Add(new Permission
        {
            Id = Guid.NewGuid(),
            Name = "other.read",
            Category = "Other",
            TenantId = anotherTenantId
        });
        await _db.SaveChangesAsync();

        var pagination = new PaginationQuery(1, 10);
        var result = await sut.GetByTenantAsync(tenantId, pagination);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPermission()
    {
        var sut = CreateSut();
        var permissionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        _db.Permissions.Add(new Permission
        {
            Id = permissionId,
            Name = "users.read",
            Category = "Users",
            TenantId = tenantId
        });
        await _db.SaveChangesAsync();

        var result = await sut.GetByIdAsync(permissionId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(permissionId);
        result.Name.Should().Be("users.read");
        result.Category.Should().Be("Users");
        result.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task DeleteAsync_Success()
    {
        var sut = CreateSut();
        var permissionId = Guid.NewGuid();
        _db.Permissions.Add(new Permission
        {
            Id = permissionId,
            Name = "users.delete",
            Category = "Users",
            TenantId = Guid.NewGuid()
        });
        await _db.SaveChangesAsync();

        await sut.DeleteAsync(permissionId);

        var permission = await _db.Permissions.FindAsync(permissionId);
        permission.Should().BeNull();
    }
}
