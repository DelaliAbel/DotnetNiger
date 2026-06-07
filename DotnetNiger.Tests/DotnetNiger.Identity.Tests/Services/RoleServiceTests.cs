using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using DotnetNiger.Identity.Application.DTOs;
using DotnetNiger.Identity.Application.Services;
using DotnetNiger.Identity.Domain.Entities;
using DotnetNiger.Identity.Infrastructure;
using FluentAssertions;
using Xunit;

#pragma warning disable CS8604

namespace DotnetNiger.Identity.Tests.Services;

public class RoleServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly IdentityDbContext _db;

    public RoleServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _roleManagerMock = CreateRoleManagerMock();

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new IdentityDbContext(options, new TenantContext());
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

    private RoleService CreateSut()
    {
        return new RoleService(
            _roleManagerMock.Object,
            _userManagerMock.Object,
            _db);
    }

    [Fact]
    public async Task CreateAsync_Success()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        var request = new CreateRoleRequest("Admin", "Administrator role", tenantId);

        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationRole>(r =>
            {
                r.Id = Guid.NewGuid();
            });

        var result = await sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("Admin");
        result.Description.Should().Be("Administrator role");
        result.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_Throws()
    {
        var sut = CreateSut();
        var request = new CreateRoleRequest("Admin", null, Guid.NewGuid());

        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateRoleName",
                Description = "Ce nom de rôle existe déjà"
            }));

        var act = () => sut.CreateAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*déjà*");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRoles()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();
        var anotherTenantId = Guid.NewGuid();

        for (int i = 1; i <= 3; i++)
        {
            _db.Roles.Add(new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = $"Role{i}",
                NormalizedName = $"ROLE{i}",
                TenantId = tenantId
            });
        }
        _db.Roles.Add(new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = "OtherRole",
            NormalizedName = "OTHERROLE",
            TenantId = anotherTenantId
        });
        await _db.SaveChangesAsync();

        var pagination = new PaginationQuery(1, 10);
        var result = await sut.GetByTenantAsync(tenantId, pagination);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsRole()
    {
        var sut = CreateSut();
        var roleId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var role = new ApplicationRole
        {
            Id = roleId,
            Name = "Admin",
            NormalizedName = "ADMIN",
            TenantId = tenantId,
            Description = "Admin role"
        };

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);

        var result = await sut.GetByIdAsync(roleId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(roleId);
        result.Name.Should().Be("Admin");
        result.Description.Should().Be("Admin role");
        result.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task UpdateAsync_Success()
    {
        var sut = CreateSut();
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole
        {
            Id = roleId,
            Name = "Admin",
            NormalizedName = "ADMIN",
            TenantId = Guid.NewGuid(),
            Description = "Old description"
        };

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var request = new UpdateRoleRequest("Updated description");
        var result = await sut.UpdateAsync(roleId, request);

        result.Description.Should().Be("Updated description");
        role.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task DeleteAsync_Success()
    {
        var sut = CreateSut();
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole
        {
            Id = roleId,
            Name = "ToDelete",
            NormalizedName = "TODELETE",
            TenantId = Guid.NewGuid()
        };

        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(x => x.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        await sut.DeleteAsync(roleId);

        _roleManagerMock.Verify(x => x.DeleteAsync(role), Times.Once);
    }

    [Fact]
    public async Task AssignToUserAsync_Success()
    {
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "test@test.com",
            Email = "test@test.com"
        };
        var role = new ApplicationRole
        {
            Id = roleId,
            Name = "Admin",
            NormalizedName = "ADMIN",
            TenantId = Guid.NewGuid()
        };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(x => x.FindByIdAsync(roleId.ToString()))
            .ReturnsAsync(role);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        await sut.AssignToUserAsync(userId, roleId);

        _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
    }
}
