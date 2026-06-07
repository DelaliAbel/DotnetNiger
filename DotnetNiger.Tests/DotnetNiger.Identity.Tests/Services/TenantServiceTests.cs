using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using OpenIddict.Abstractions;
using DotnetNiger.Identity.Application.DTOs;
using DotnetNiger.Identity.Application.Exceptions;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using FluentAssertions;
using Xunit;

#pragma warning disable CS8604

namespace DotnetNiger.Identity.Tests.Services;

public class TenantServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IOpenIddictApplicationManager> _applicationManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IdentityDbContext _db;
    private readonly TenantApiKeyService _apiKeyService;

    public TenantServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _roleManagerMock = CreateRoleManagerMock();
        _applicationManagerMock = new Mock<IOpenIddictApplicationManager>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Admin:DefaultPassword"]).Returns("Test123$");

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new IdentityDbContext(options, new TenantContext());
        _apiKeyService = new TenantApiKeyService(_db);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = Mock.Of<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<RoleManager<ApplicationRole>> CreateRoleManagerMock()
    {
        var store = Mock.Of<IRoleStore<ApplicationRole>>();
        return new Mock<RoleManager<ApplicationRole>>(
            store, null!, null!, null!, null!);
    }

    private TenantService CreateSut()
    {
        return new TenantService(
            _db,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _applicationManagerMock.Object,
            _apiKeyService,
            _configurationMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Success()
    {
        var sut = CreateSut();
        var request = new CreateTenantRequest("Test Tenant", "test-tenant", "A test tenant");

        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Test123$"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Tenant");
        result.Slug.Should().Be("test-tenant");
        result.Description.Should().Be("A test tenant");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_DuplicateSlug_Throws()
    {
        var sut = CreateSut();
        _db.Tenants.Add(new Tenant { Id = Guid.NewGuid(), Name = "Existing", Slug = "existing" });
        await _db.SaveChangesAsync();

        var request = new CreateTenantRequest("New", "existing", null);

        var act = () => sut.CreateAsync(request);

        await act.Should().ThrowAsync<SlugAlreadyExistsException>()
            .WithMessage("*existing*");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTenant()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        _db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Test",
            Slug = "test",
            Description = "Desc",
            IsActive = true
        });
        await _db.SaveChangesAsync();

        var result = await sut.GetByIdAsync(tenantId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(tenantId);
        result.Name.Should().Be("Test");
        result.Slug.Should().Be("test");
        result.Description.Should().Be("Desc");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAll()
    {
        var sut = CreateSut();
        for (int i = 1; i <= 3; i++)
        {
            _db.Tenants.Add(new Tenant
            {
                Id = Guid.NewGuid(),
                Name = $"Tenant {i}",
                Slug = $"tenant-{i}",
                IsActive = true
            });
        }
        await _db.SaveChangesAsync();

        var pagination = new PaginationQuery(1, 10);
        var result = await sut.GetAllAsync(pagination);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task UpdateAsync_Success()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        _db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Original",
            Slug = "original",
            Description = "Old desc",
            IsActive = true
        });
        await _db.SaveChangesAsync();

        var request = new UpdateTenantRequest("Updated", "New desc", null);
        var result = await sut.UpdateAsync(tenantId, request);

        result.Name.Should().Be("Updated");
        result.Description.Should().Be("New desc");
        var tenant = await _db.Tenants.FindAsync(tenantId);
        tenant!.Name.Should().Be("Updated");
        tenant.Description.Should().Be("New desc");
    }

    [Fact]
    public async Task ToggleActiveAsync_Success()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        _db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "Test",
            Slug = "test",
            IsActive = true
        });
        await _db.SaveChangesAsync();

        var request = new UpdateTenantRequest(null, null, false);
        var result = await sut.UpdateAsync(tenantId, request);

        result.IsActive.Should().BeFalse();

        request = new UpdateTenantRequest(null, null, true);
        result = await sut.UpdateAsync(tenantId, request);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Success()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        _db.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = "ToDelete",
            Slug = "to-delete",
            IsActive = true
        });
        await _db.SaveChangesAsync();

        await sut.DeleteAsync(tenantId);

        var tenant = await _db.Tenants.FindAsync(tenantId);
        tenant.Should().BeNull();
    }
}
