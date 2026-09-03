using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetNiger.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationExpiresAt",
                table: "NewsletterSubscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationToken",
                table: "NewsletterSubscriptions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "NewsletterSubscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "NewsletterSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptions_ConfirmationToken",
                table: "NewsletterSubscriptions",
                column: "ConfirmationToken",
                unique: true,
                filter: "[ConfirmationToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsletterSubscriptions_ConfirmationToken",
                table: "NewsletterSubscriptions");

            migrationBuilder.DropColumn(
                name: "ConfirmationExpiresAt",
                table: "NewsletterSubscriptions");

            migrationBuilder.DropColumn(
                name: "ConfirmationToken",
                table: "NewsletterSubscriptions");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "NewsletterSubscriptions");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "NewsletterSubscriptions");
        }
    }
}
