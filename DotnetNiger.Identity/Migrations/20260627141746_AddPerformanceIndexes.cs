using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetNiger.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserConsents_CreatedAt",
                table: "UserConsents");

            migrationBuilder.DropIndex(
                name: "IX_UserConsents_UserId",
                table: "UserConsents");

            migrationBuilder.DropIndex(
                name: "IX_LoginHistories_CreatedAt",
                table: "LoginHistories");

            migrationBuilder.DropIndex(
                name: "IX_LoginHistories_UserId",
                table: "LoginHistories");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserId_CreatedAt",
                table: "UserConsents",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_UserId_CreatedAt",
                table: "LoginHistories",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalServices_IsActive_Status",
                table: "ExternalServices",
                columns: new[] { "IsActive", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserConsents_UserId_CreatedAt",
                table: "UserConsents");

            migrationBuilder.DropIndex(
                name: "IX_LoginHistories_UserId_CreatedAt",
                table: "LoginHistories");

            migrationBuilder.DropIndex(
                name: "IX_ExternalServices_IsActive_Status",
                table: "ExternalServices");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_CreatedAt",
                table: "UserConsents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserConsents_UserId",
                table: "UserConsents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_CreatedAt",
                table: "LoginHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_UserId",
                table: "LoginHistories",
                column: "UserId");
        }
    }
}
