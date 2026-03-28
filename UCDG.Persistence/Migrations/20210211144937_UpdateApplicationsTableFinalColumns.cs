using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class UpdateApplicationsTableFinalColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicantProgress",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DHETFundsRequested",
                table: "Applications",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DepartmentContribution",
                table: "Applications",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FacultyContibution",
                table: "Applications",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FinancialMotivation",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutputMeasure",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ResearchFundsContribution",
                table: "Applications",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "Applications",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantProgress",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "DHETFundsRequested",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "DepartmentContribution",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "FacultyContibution",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "FinancialMotivation",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "OutputMeasure",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ResearchFundsContribution",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "Applications");
        }
    }
}
