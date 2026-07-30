using System;
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
            migrationBuilder.DropForeignKey(
                name: "FK_SocialLinks_Members_MemberId1",
                table: "SocialLinks");

            migrationBuilder.DropIndex(
                name: "IX_SocialLinks_MemberId1",
                table: "SocialLinks");

            migrationBuilder.DropColumn(
                name: "MemberId1",
                table: "SocialLinks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ContactMessages");

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
