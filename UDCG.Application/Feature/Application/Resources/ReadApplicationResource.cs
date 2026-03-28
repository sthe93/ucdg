using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Feature.DocumentSignOff.Resources;
using UDCG.Application.Feature.FundingCalls.Resources;
using UDCG.Application.Feature.Roles.Resources;

namespace UDCG.Application.Feature.Application.Resources
{
    public class ReadApplicationResource
    {
        public ReadApplicationResource()
        {
            Applicant = new ReadUserViewModelResource();
            ApplicationStatus = new ReadApplicationStatusResouce();
            FundingCalls = new ReadFundingCallsResource();
        //    DocumentSignOff = new ReadDocumentSignOffViewModel(); 
        }
        public int Id { get; set; }
        public ReadUserViewModelResource Applicant { get; set; }
        public ReadFundingCallsResource FundingCalls { get; set; }
        public ReadApplicationStatusResouce ApplicationStatus { get; set; }
       //public ReadDocumentSignOffViewModel DocumentSignOff { get; set; }
        public DateTime FundingStartDate { get; set; }
        public DateTime FundingEndDate { get; set; }
        public DateTime ApplicationStartDate { get; set; }
        public DateTime? ApplicationEndDate { get; set; }
        public string ApplicantCategory { get; set; }
        public string AppointmentCategory { get; set; }
        public string LastSavedStep { get; set; }
        public string StudyingTowards { get; set; }
        public string FirstYearRegistration { get; set; }
        public string PlannedGraduationYear { get; set; }
        public string Describe { get; set; }
        public string AppointmentDescribe { get; set; }
        public string SupportRequired { get; set; }
        public string AppointmentOption { get; set; }
        public string FinancialSupport { get; set; }
        public string CareerFinancialSupport { get; set; }
        public string CareerTeachingRelief { get; set; }
        public string FinancialMotivation { get; set; }
        public string ApplicantProgress { get; set; }
        public string OutputMeasure { get; set; }
        public string FacultyContibution { get; set; }
        public string OtherFunding { get; set; }
        public string DepartmentContribution { get; set; }
        public string ResearchFundsContribution { get; set; }
        public string? DHETFundsRequested { get; set; }
        public string TotalCost { get; set; }
        public string ApprovedAmount { get; set; }
        public string ReferenceNumber { get; set; }
        public int NumberOfDocuments { get; set; }
        public string LastModifierUsername { get; set; }
        public bool IsAcknowledge { get; set; }
        public string ApprovedAs { get; set; }
        public string FundAdminComment { get; set; }
        public string FundAdminApprovedAmount { get; set; }
        public bool ProgressReportComplete { get; set; }
        public bool ViewProgressReportComment { get; set; }
        public int ReportId { get; set; }
    }
}
