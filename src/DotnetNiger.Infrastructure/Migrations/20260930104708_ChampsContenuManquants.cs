using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetNiger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChampsContenuManquants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "CommunityEvents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Profession",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "CommunityEvents");

            migrationBuilder.DropColumn(
                name: "Profession",
                table: "AspNetUsers");
        }
    }
}
