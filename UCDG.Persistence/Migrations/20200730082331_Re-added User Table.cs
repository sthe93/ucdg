using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class ReaddedUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    Surname = table.Column<string>(nullable: true),
                    Nationality = table.Column<string>(nullable: true),
                    IdPassportNumber = table.Column<string>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: false),
                    EmailAddress = table.Column<string>(nullable: true),
                    AlternativeEmailAddress = table.Column<string>(nullable: true),
                    CellPhoneNumber = table.Column<string>(nullable: true),
                    TelephoneNumber = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsProfileCompleted = table.Column<bool>(nullable: false),
                    Campus = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    StaffNumber = table.Column<string>(nullable: true),
                    Position = table.Column<string>(nullable: true),
                    IsAcademic = table.Column<bool>(nullable: false),
                    Faculty = table.Column<string>(nullable: true),
                    Department = table.Column<string>(nullable: true),
                    Race = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: true),
                    OtherProgramme = table.Column<bool>(nullable: false),
                    OtherProgrammeName = table.Column<string>(nullable: true),
                    OtherFundSource = table.Column<bool>(nullable: false),
                    OtherFundSourceName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
