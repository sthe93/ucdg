using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class Added_LasthgModifierUsername_LastModifiedDate_SubmittedDate_on_Applications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Applications",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastModifierUsername",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedDate",
                table: "Applications",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "LastModifierUsername",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "SubmittedDate",
                table: "Applications");
        }
    }
}
