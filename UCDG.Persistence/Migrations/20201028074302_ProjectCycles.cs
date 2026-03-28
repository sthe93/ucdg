using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class ProjectCycles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectCyclesId",
                table: "Projects",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectCycles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Period = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectCycles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectCyclesId",
                table: "Projects",
                column: "ProjectCyclesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ProjectCycles_ProjectCyclesId",
                table: "Projects",
                column: "ProjectCyclesId",
                principalTable: "ProjectCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectCycles_ProjectCyclesId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "ProjectCycles");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ProjectCyclesId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectCyclesId",
                table: "Projects");
        }
    }
}
