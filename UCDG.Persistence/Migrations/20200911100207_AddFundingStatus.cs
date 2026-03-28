using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class AddFundingStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "FundingCalls");

            migrationBuilder.AddColumn<int>(
                name: "FundingCallStatusId",
                table: "FundingCalls",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FundingCallStatus",
                columns: table => new
                {
                    FundingCallStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingCallStatus", x => x.FundingCallStatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FundingCalls_FundingCallStatusId",
                table: "FundingCalls",
                column: "FundingCallStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingCalls_FundingCallStatus_FundingCallStatusId",
                table: "FundingCalls",
                column: "FundingCallStatusId",
                principalTable: "FundingCallStatus",
                principalColumn: "FundingCallStatusId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundingCalls_FundingCallStatus_FundingCallStatusId",
                table: "FundingCalls");

            migrationBuilder.DropTable(
                name: "FundingCallStatus");

            migrationBuilder.DropIndex(
                name: "IX_FundingCalls_FundingCallStatusId",
                table: "FundingCalls");

            migrationBuilder.DropColumn(
                name: "FundingCallStatusId",
                table: "FundingCalls");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FundingCalls",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
