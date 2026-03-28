using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class Added_DocumentSignOff_Table2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentSignOffs",
                columns: table => new
                {
                    DocumentSignOffID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    UserFullName = table.Column<string>(nullable: true),
                    DocumentType = table.Column<string>(nullable: true),
                    SignedDate = table.Column<DateTime>(nullable: false),
                    UserRoleName = table.Column<string>(nullable: true),
                    ReferenceNumber = table.Column<string>(nullable: true),
                    ApplicationsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSignOffs", x => x.DocumentSignOffID);
                    table.ForeignKey(
                        name: "FK_DocumentSignOffs_Applications_ApplicationsId",
                        column: x => x.ApplicationsId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentSignOffs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignOffs_ApplicationsId",
                table: "DocumentSignOffs",
                column: "ApplicationsId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignOffs_UserId",
                table: "DocumentSignOffs",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentSignOffs");
        }
    }
}
