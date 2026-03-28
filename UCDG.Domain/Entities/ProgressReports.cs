using System; 

namespace UCDG.Domain.Entities
{
    public class ProgressReports
    {
        
        public int Id { get; set; } 
        public bool IsComplete { get; set; }
        public DateTime CreatedDate { get; set; }
        public Applications Application { get; set; }
        public ProgressReportStatus ProgressReportStatus { get; set; }
    
        //Qualification - In Progress - Step 2
        public bool IsQualificationInPrgress { get; set; }
        public string QualificationName { get; set; }
        public string QualificationInPrgressFieldOfStudy { get; set; }
        public string QualificationInPrgressTitleofThesis { get; set; }
        public string QualificationInPrgressInstitution { get; set; }
        public string QualificationInPrgressGraduationYear { get; set; }
        //public IFormFile UploadedDoc { get; set; }
        //public CreateDocumentViewModel Document { get; set; }


        //Qualification - Graduated - Step 3
        public bool IsQualificationGraduated { get; set; }
        public string QualificationGraduatedName { get; set; }
        public string QualificationGraduatedFieldOfStudy { get; set; }
        public string QualificationGraduatedTitleofThesis { get; set; }
        public string QualificationGraduatedInstitution { get; set; }
        public string QualificationGraduatedYear { get; set; }

        //Teaching Relief Appointment - Step 4
        public bool IsReliefAppointment { get; set; }

        //Research Publications (Outputs) - Step 5
        public bool IsResearchPublication { get; set; }
        public string ResearchAccreditedJournal { get; set; }
        public string ResearchAccreditedChapter { get; set; }
        public string ResearchAccreditedBook { get; set; }
        public string ResearchAccreditedConference { get; set; }

        //Research Projects (Outputs) - Step 6
        public bool IsResearchProject { get; set; }
        public string ResearchProjectSupport { get; set; }
        public string Activities { get; set; }
        public string Outputs { get; set; }
        public string Outcome { get; set; }

        //Collaborative Projects (Outputs) - Step 7
        public bool IsCollaborativeProject { get; set; }
        public string CollaborativeProjectSupported { get; set; }
        public string CollaborativeActivities { get; set; }
        public string CollaborativeOutputs { get; set; }
        public string CollaborativeOutcome { get; set; }
        public int ApplicationId { get; set; }
    }
}
