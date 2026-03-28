using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class AddApplicationSupportRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportRequired",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportRequired", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationSupportRequired",
                columns: table => new
                {
                    ApplicationsId = table.Column<int>(nullable: false),
                    SupportRequiredId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSupportRequired", x => new { x.ApplicationsId, x.SupportRequiredId });
                    table.ForeignKey(
                        name: "FK_ApplicationSupportRequired_Applications_ApplicationsId",
                        column: x => x.ApplicationsId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationSupportRequired_SupportRequired_SupportRequiredId",
                        column: x => x.SupportRequiredId,
                        principalTable: "SupportRequired",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSupportRequired_SupportRequiredId",
                table: "ApplicationSupportRequired",
                column: "SupportRequiredId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationSupportRequired");

            migrationBuilder.DropTable(
                name: "SupportRequired");
        }
    }
}
