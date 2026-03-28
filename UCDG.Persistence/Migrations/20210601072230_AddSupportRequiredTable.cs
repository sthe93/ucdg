using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class AddSupportRequiredTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSignOffs_Applications_ApplicationsId",
                table: "DocumentSignOffs");

            migrationBuilder.AlterColumn<int>(
                name: "ApplicationsId",
                table: "DocumentSignOffs",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSignOffs_Applications_ApplicationsId",
                table: "DocumentSignOffs",
                column: "ApplicationsId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSignOffs_Applications_ApplicationsId",
                table: "DocumentSignOffs");

            migrationBuilder.AlterColumn<int>(
                name: "ApplicationsId",
                table: "DocumentSignOffs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSignOffs_Applications_ApplicationsId",
                table: "DocumentSignOffs",
                column: "ApplicationsId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
