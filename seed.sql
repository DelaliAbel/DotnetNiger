-- ============================================================
-- Seed initial pour DotnetNiger Identity (SQL Server)
-- Execute une seule fois sur une base vide
-- Usage: sqlcmd -S localhost -d DotnetNigerIdentity -i seed.sql
-- ============================================================

-- Tenant par defaut
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Slug = 'platform')
BEGIN
    INSERT INTO Tenants (Id, Name, Slug, Description, IsActive, CreatedAt)
    VALUES (NEWID(), 'DotnetNiger Community', 'platform', 'Tenant principal DotnetNiger', 1, GETUTCDATE());
END
GO

-- Roles
DECLARE @TenantId UNIQUEIDENTIFIER = (SELECT Id FROM Tenants WHERE Slug = 'platform');

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SuperAdmin')
    INSERT INTO AspNetRoles (Id, TenantId, Name, NormalizedName, Description, CreatedAt)
    VALUES (NEWID(), @TenantId, 'SuperAdmin', 'SUPERADMIN', 'Super administrateur de la plateforme', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
    INSERT INTO AspNetRoles (Id, TenantId, Name, NormalizedName, Description, CreatedAt)
    VALUES (NEWID(), @TenantId, 'Admin', 'ADMIN', 'Administrateur de la plateforme', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'User')
    INSERT INTO AspNetRoles (Id, TenantId, Name, NormalizedName, Description, CreatedAt)
    VALUES (NEWID(), @TenantId, 'User', 'USER', 'Utilisateur standard', GETUTCDATE());

GO

-- Pour creer le compte admin, lancer l'app avec --seed :
--   dotnet run --project DotnetNiger.Identity -- --seed
-- (Admin:DefaultPassword doit etre configure dans appsettings.Production.json)
PRINT 'Seed termine. Pour creer le compte admin, lancez: dotnet run --project DotnetNiger.Identity -- --seed';
GO
