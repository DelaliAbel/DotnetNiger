using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetNiger.Community.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsletterSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerificationToken = table.Column<string>(type: "TEXT", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UnsubscribedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MemberId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsletterSubscriptions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Resources_IsApproved_CreatedAt",
                table: "Resources",
                columns: new[] { "IsApproved", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatedAt",
                table: "Posts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_IsPublished_PublishedAt",
                table: "Posts",
                columns: new[] { "IsPublished", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsPublished_StartDate",
                table: "Events",
                columns: new[] { "IsPublished", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptions_Email",
                table: "NewsletterSubscriptions",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptions_IsActive_IsVerified",
                table: "NewsletterSubscriptions",
                columns: new[] { "IsActive", "IsVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscriptions_MemberId",
                table: "NewsletterSubscriptions",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsletterSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Resources_IsApproved_CreatedAt",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedAt",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CreatedAt",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_IsPublished_PublishedAt",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Events_IsPublished_StartDate",
                table: "Events");
        }
    }
}
