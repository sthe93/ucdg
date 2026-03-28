using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UCDG.Persistence.Migrations
{
    public partial class CreateReportTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        { 
            migrationBuilder.CreateTable(
                name: "ProgressReports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsComplete = table.Column<bool>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ApplicationId = table.Column<int>(nullable: true),
                    IsQualificationInPrgress = table.Column<bool>(nullable: false),
                    QualificationName = table.Column<string>(nullable: true),
                    QualificationInPrgressFieldOfStudy = table.Column<string>(nullable: true),
                    QualificationInPrgressTitleofThesis = table.Column<string>(nullable: true),
                    QualificationInPrgressInstitution = table.Column<string>(nullable: true),
                    QualificationInPrgressGraduationYear = table.Column<string>(nullable: true),
                    IsQualificationGraduated = table.Column<bool>(nullable: false),
                    QualificationGraduatedName = table.Column<string>(nullable: true),
                    QualificationGraduatedFieldOfStudy = table.Column<string>(nullable: true),
                    QualificationGraduatedTitleofThesis = table.Column<string>(nullable: true),
                    QualificationGraduatedInstitution = table.Column<string>(nullable: true),
                    QualificationGraduatedYear = table.Column<string>(nullable: true),
                    IsReliefAppointment = table.Column<bool>(nullable: false),
                    IsResearchPublication = table.Column<bool>(nullable: false),
                    ResearchAccreditedJournal = table.Column<string>(nullable: true),
                    ResearchAccreditedChapter = table.Column<string>(nullable: true),
                    ResearchAccreditedBook = table.Column<string>(nullable: true),
                    ResearchAccreditedConference = table.Column<string>(nullable: true),
                    IsResearchProject = table.Column<bool>(nullable: false),
                    ResearchProjectSupport = table.Column<string>(nullable: true),
                    Activities = table.Column<string>(nullable: true),
                    Outputs = table.Column<string>(nullable: true),
                    Outcome = table.Column<string>(nullable: true),
                    IsCollaborativeProject = table.Column<bool>(nullable: false),
                    CollaborativeProjectSupported = table.Column<string>(nullable: true),
                    CollaborativeActivities = table.Column<string>(nullable: true),
                    CollaborativeOutputs = table.Column<string>(nullable: true),
                    CollaborativeOutcome = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressReports_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReports_ApplicationId",
                table: "ProgressReports",
                column: "ApplicationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropTable(
                name: "ProgressReports");

        }
    }
}
