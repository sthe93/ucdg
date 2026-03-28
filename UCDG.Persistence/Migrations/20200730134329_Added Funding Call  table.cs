using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class AddedFundingCalltable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                 migrationBuilder.CreateTable(
                name: "FundingCalls",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FundingCallName = table.Column<string>(nullable: true),
                    ProjectName = table.Column<string>(nullable: true),
                    ShortDescription = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingCalls", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundingCalls");
    


        }
    }
}
