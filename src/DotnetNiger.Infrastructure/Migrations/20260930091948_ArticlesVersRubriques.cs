using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetNiger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArticlesVersRubriques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La colonne RubriqueId est d'abord ajoutée nullable : la base peut déjà
            // contenir des articles, or une colonne NOT NULL avec une valeur par défaut
            // exigerait un identifiant de rubrique valide, qui n'existe pas encore.

            migrationBuilder.AddColumn<Guid>(
                name: "RubriqueId",
                table: "Articles",
                type: "uniqueidentifier",
                nullable: true);

            // Les trois catégories de l'ancien enum deviennent des rubriques. Le nom de
            // la rubrique reprend l'ancien libellé : un article garde lisible la catégorie
            // qu'il portait, et l'administrateur pourra la réorganiser ensuite.
            // L'amorçage a déjà peuplé la table sur une base neuve : insérer ici les
            // mêmes rubriques violerait l'unicité du slug et interromprait le
            // démarrage. Elles ne sont créées que si elles manquent.
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM [Rubriques] WHERE [Slug] = N'nouveautes')
                BEGIN
                    DECLARE @nouveautes uniqueidentifier = NEWID();
                    DECLARE @projets uniqueidentifier = NEWID();
                    DECLARE @interviews uniqueidentifier = NEWID();

                    INSERT INTO [Rubriques] ([Id], [Couleur], [Nom], [Ordre], [Slug])
                    SELECT @nouveautes, N'violet', N'Nouveautés de .NET', 10, N'nouveautes'
                    WHERE NOT EXISTS (SELECT 1 FROM [Rubriques] WHERE [Slug] = N'projets')
                      AND NOT EXISTS (SELECT 1 FROM [Rubriques] WHERE [Slug] = N'interviews');

                    INSERT INTO [Rubriques] ([Id], [Couleur], [Nom], [Ordre], [Slug])
                    SELECT @projets, N'vert', N'Projets locaux', 110, N'projets'
                    WHERE NOT EXISTS (SELECT 1 FROM [Rubriques] WHERE [Slug] = N'projets');

                    INSERT INTO [Rubriques] ([Id], [Couleur], [Nom], [Ordre], [Slug])
                    SELECT @interviews, N'orange', N'Interviews', 120, N'interviews'
                    WHERE NOT EXISTS (SELECT 1 FROM [Rubriques] WHERE [Slug] = N'interviews');
                END

                UPDATE [Articles] SET [RubriqueId] =
                    COALESCE(
                        (SELECT TOP 1 [Id] FROM [Rubriques] WHERE [Slug] = CASE [Category]
                            WHEN N'LocalProjects' THEN N'projets'
                            WHEN N'Interview' THEN N'interviews'
                            ELSE N'nouveautes' END),
                        (SELECT TOP 1 [Id] FROM [Rubriques] ORDER BY [Ordre]))
                WHERE [RubriqueId] IS NULL;
                """);

            migrationBuilder.Sql(
                "ALTER TABLE [Articles] ALTER COLUMN [RubriqueId] uniqueidentifier NOT NULL;");

            migrationBuilder.Sql(
                "IF COL_LENGTH('Articles', 'Category') IS NOT NULL ALTER TABLE [Articles] DROP COLUMN [Category];");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_RubriqueId",
                table: "Articles",
                column: "RubriqueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Rubriques_RubriqueId",
                table: "Articles",
                column: "RubriqueId",
                principalTable: "Rubriques",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Retour arrière impossible sans décision editorial : rattacher chaque article à
            // l'une des trois anciennes catégories demanderait de savoir laquelle, et
            // l'information n'a pas été conservée. Une restauration de sauvegarde est
            // préférable à une déduction automatique.
            throw new NotSupportedException(
                "Bascule des articles vers les rubriques irréversible : restauration de sauvegarde requise.");
        }
    }
}
