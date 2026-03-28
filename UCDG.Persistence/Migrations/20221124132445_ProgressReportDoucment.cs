using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class ProgressReportDoucment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgressReportDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filename = table.Column<string>(nullable: true),
                    ContentType = table.Column<string>(nullable: true),
                    DocumentExtention = table.Column<string>(nullable: true),
                    DocumentFile = table.Column<byte[]>(nullable: true),
                    UploadType = table.Column<string>(nullable: true),
                    ProgressReportId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressReportDocuments", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_ProgressReportDocuments_ProgressReports_ProgressReportId",
                        column: x => x.ProgressReportId,
                        principalTable: "ProgressReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReportDocuments_ProgressReportId",
                table: "ProgressReportDocuments",
                column: "ProgressReportId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressReportDocuments");
        }
    }
}
