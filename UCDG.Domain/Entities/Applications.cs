using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class Applications
    {
            public int? UserId { get; set; }        // 🔥 ADD THIS

        public int Id { get; set; }
        public User Applicant { get; set; }
        public string Username { get; set; }
        public FundingCalls FundingCalls { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
        public DateTime FundingStartDate { get; set; }
        public DateTime FundingEndDate { get; set; }
        public DateTime ApplicationStartDate { get; set; }
        public DateTime? ApplicationEndDate { get; set; }
        public string ApplicantCategory { get; set; }
        public string AppointmentCategory { get; set; }
        public string LastSavedStep { get; set; }

        //Improvement of Staff Qualifications Step
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

        //Final Step
        public string FinancialMotivation { get; set; }
        public string ApplicantProgress { get; set; }
        public string OutputMeasure { get; set; } 
        public string FacultyContibution { get; set; } 
        public string DepartmentContribution { get; set; } 
        public string OtherFunding { get; set; }
        public string ResearchFundsContribution { get; set; } 
        public string DHETFundsRequested { get; set; } = "0.00";
        public string TotalCost { get; set; } 
        public string ApprovedAmount { get; set; } 
        public string FundAdminApprovedAmount { get; set; }
        public string FundAdminComment { get; set; }
        public string SIAComment { get; set; }
        public string ReferenceNumber { get; set; }
        public Guid ReferenceId { get; set; }
        public string LastModifierUsername { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime SubmittedDate { get; set; }
        //  public object DocumentSignOffs { get; set; }
        public string CostCentreName { get; set; }
        public string CostCentreNumber { get; set; }
        public string PreviousFundingYear { get; set; }
        public string PreviousFundingAmount { get; set; }
        public string PreviousFundingOutcome { get; set; }
        public string RFIByFundAdmin { get; set; }

        public int? ApplicantUserStoreUserId { get; set; }
        public string CurrentApproverStaffNumber { get; set; }
        public DateTime? LastApproverRefreshDateUtc { get; set; }
        public int? LastApproverRefreshByUserStoreUserId { get; set; }

        public int FundingCallsId { get; set; }
        public int ApplicationStatusId { get; set; }
        public bool? FlightsChooseCheapest { get; set; }
        public string FlightsCheapestExplanation { get; set; }
        public bool? AccomChooseCheapest { get; set; }
        public string AccomCheapestExplanation { get; set; }
        public string OtherFundingSource { get; set; }
        public string FieldOfStudy { get; set; }
        public string TitleOfThesis { get; set; }

    }
}
