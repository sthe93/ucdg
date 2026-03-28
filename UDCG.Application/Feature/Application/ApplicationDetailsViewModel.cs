using System;
using UDCG.Application.Feature.Roles.Resources;

namespace UDCG.Application.Feature.Application
{
    public class ApplicationDetailsViewModel
    {
        public int Id { get; set; }
        public int FundingCallDetailsId { get; set; }
        public int ApplicationStatusId { get; set; }
        public int UserId { get; set; }
        public DateTime FundingStartDate { get; set; }
        public DateTime FundingEndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ApplicantCategory { get; set; }
        public string AppointmentCategory { get; set; }
        public string LastSavedStep { get; set; }
        public string[] ProjectId { get; set; }

        //Improvement of Staff Qualifications Step
        public string StudyingTowards { get; set; }
        public string FirstYearRegistration { get; set; }
        public string PlannedGraduationYear { get; set; }
        public string Describe { get; set; }
        public string AppointmentDescribe { get; set; }
        public string[] SupportRequired { get; set; }
        public string[] AppointmentOption { get; set; }
        public string[] FinancialSupport { get; set; }
        public string[] CareerFinancialSupport { get; set; }
        public string[] CareerTeachingRelief { get; set; }

        //Final Step
        public string FinancialMotivation { get; set; }
        public string ApplicantProgress { get; set; }
        public string OutputMeasure { get; set; }
        public string FacultyContibution { get; set; }
        public string DepartmentContribution { get; set; }
        public string OtherFunding { get; set; }
        
        public string ResearchFundsContribution { get; set; }
        public string DHETFundsRequested { get; set; }
        public string TotalCost { get; set; }
        public string ApprovedAmount { get; set; }
        public string FundAdminApprovedAmount { get; set; }
        public string LastModifierUsername { get; set; }
        public string FundAdminComment { get; set; }
        public string SIAComment { get; set; }
        public string ReferenceNumber { get; set; }
        //adding funding name 
        public string FundingCallDetailName { get; set; }
        public ReadUserViewModelResource UserDetails { get; set; }
        public Guid ReferenceId { get; set; }
        public string ProjectName { get; set; }
        // for signing 
        public bool IsAcknowledge { get; set; }
        public DateTime LastModifiedDate { get; set; }

        public string CostCentreName { get; set; }
        public string CostCentreNumber { get; set; }
        public string PreviousFundingYear { get; set; }
        public string PreviousFundingAmount { get; set; }
        public string PreviousFundingOutcome { get; set; }
        public bool? FlightsChooseCheapest { get; set; }
        public string FlightsCheapestExplanation { get; set; }
        public bool? AccomChooseCheapest { get; set; }
        public string AccomCheapestExplanation { get; set; }
        public string OtherFundingSource { get; set; }
        public string CurrentApproverStaffNumber { get; set; }
        public string FieldOfStudy { get; set; }
        public string TitleOfThesis { get; set; }
    }
}
