using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class Added_HODStaffNUmber_ViceDeanStaffNUmber_User : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HODStaffNUmber",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ViceDeanStaffNUmber",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HODStaffNUmber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ViceDeanStaffNUmber",
                table: "Users");
        }
    }
}
