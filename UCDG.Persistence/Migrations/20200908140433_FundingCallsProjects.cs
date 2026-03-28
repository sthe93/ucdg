using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class FundingCallsProjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingCalls_Projects_ProjectsId",
                table: "FundingCalls");

            migrationBuilder.DropIndex(
                name: "IX_FundingCalls_ProjectsId",
                table: "FundingCalls");

            migrationBuilder.DropColumn(
                name: "ProjectsId",
                table: "FundingCalls");

            migrationBuilder.CreateTable(
                name: "FundingCallsProjects",
                columns: table => new
                {
                    FundingCallsId = table.Column<int>(nullable: false),
                    ProjectsId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingCallsProjects", x => new { x.FundingCallsId, x.ProjectsId });
                    table.ForeignKey(
                        name: "FK_FundingCallsProjects_FundingCalls_FundingCallsId",
                        column: x => x.FundingCallsId,
                        principalTable: "FundingCalls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingCallsProjects_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundingCallsProjects_ProjectsId",
                table: "FundingCallsProjects",
                column: "ProjectsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundingCallsProjects");

            migrationBuilder.AddColumn<int>(
                name: "ProjectsId",
                table: "FundingCalls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_FundingCalls_ProjectsId",
                table: "FundingCalls",
                column: "ProjectsId");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingCalls_Projects_ProjectsId",
                table: "FundingCalls",
                column: "ProjectsId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
