using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetNiger.Api.Migrations
{
    /// <inheritdoc />
    public partial class FinalCleanupMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SocialLinks_Members_MemberId1')
                    ALTER TABLE [SocialLinks] DROP CONSTRAINT [FK_SocialLinks_Members_MemberId1];
            ");
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SocialLinks_MemberId1' AND object_id = OBJECT_ID('SocialLinks'))
                    DROP INDEX [IX_SocialLinks_MemberId1] ON [SocialLinks];
            ");
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SocialLinks' AND COLUMN_NAME = 'MemberId1')
                    ALTER TABLE [SocialLinks] DROP COLUMN [MemberId1];
            ");
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ContactMessages' AND COLUMN_NAME = 'Name')
                    ALTER TABLE [ContactMessages] DROP COLUMN [Name];
            ");
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "ContactMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MemberId1",
                table: "SocialLinks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "ContactMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ContactMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SocialLinks_MemberId1",
                table: "SocialLinks",
                column: "MemberId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SocialLinks_Members_MemberId1",
                table: "SocialLinks",
                column: "MemberId1",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
