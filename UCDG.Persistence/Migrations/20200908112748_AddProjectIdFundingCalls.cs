using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class AddProjectIdFundingCalls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "FundingCalls");

            migrationBuilder.AddColumn<int>(
                name: "ProjectsId",
                table: "FundingCalls",
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
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "FundingCalls",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
