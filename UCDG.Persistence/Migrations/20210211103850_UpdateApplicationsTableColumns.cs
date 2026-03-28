using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class UpdateApplicationsTableColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CareerFinancialSupport",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CareerTeachingRelief",
                table: "Applications",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CareerFinancialSupport",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "CareerTeachingRelief",
                table: "Applications");
        }
    }
}
