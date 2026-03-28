using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class AddedHODViceDeanandDisabilityUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Disability",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HOD",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViceDean",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disability",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HOD",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ViceDean",
                table: "Users");
        }
    }
}
