using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class CreatePersistentSettingsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "MonthTotal",
                table: "Payments",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "PersistentSettings",
                columns: table => new
                {
                    PersistentSettingsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersistentSettingsKey = table.Column<string>(nullable: true),
                    PersistentSettingsValue = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersistentSettings", x => x.PersistentSettingsId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersistentSettings");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthTotal",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double));
        }
    }
}
