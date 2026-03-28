using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class ProgressReportStatusTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProgressReportStatusId",
                table: "ProgressReports",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProgressReportStatus",
                columns: table => new
                {
                    ProgressReportStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressReportStatus", x => x.ProgressReportStatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReports_ProgressReportStatusId",
                table: "ProgressReports",
                column: "ProgressReportStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressReports_ProgressReportStatus_ProgressReportStatusId",
                table: "ProgressReports",
                column: "ProgressReportStatusId",
                principalTable: "ProgressReportStatus",
                principalColumn: "ProgressReportStatusId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressReports_ProgressReportStatus_ProgressReportStatusId",
                table: "ProgressReports");

            migrationBuilder.DropTable(
                name: "ProgressReportStatus");

            migrationBuilder.DropIndex(
                name: "IX_ProgressReports_ProgressReportStatusId",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "ProgressReportStatusId",
                table: "ProgressReports");
        }
    }
}
