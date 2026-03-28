using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class UpdateApplicationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppointmentDescribe",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppointmentOption",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Describe",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstYearRegistration",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlannedGraduationYear",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudyingTowards",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportRequired",
                table: "Applications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentDescribe",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "AppointmentOption",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Describe",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "FirstYearRegistration",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PlannedGraduationYear",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "StudyingTowards",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "SupportRequired",
                table: "Applications");
        }
    }
}
