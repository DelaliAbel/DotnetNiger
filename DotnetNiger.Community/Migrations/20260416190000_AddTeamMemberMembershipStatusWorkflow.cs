using System;
using DotnetNiger.Community.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotnetNiger.Community.Migrations
{
    [DbContext(typeof(CommunityDbContext))]
    [Migration("20260416190000_AddTeamMemberMembershipStatusWorkflow")]
    public partial class AddTeamMemberMembershipStatusWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MembershipStatus",
                table: "Members",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Members",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "Members",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_MembershipStatus",
                table: "Members",
                column: "MembershipStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_MembershipStatus",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "MembershipStatus",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Members");
        }
    }
}
