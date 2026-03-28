using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;
using UCDG.Domain.Entities;

namespace UDCG.Application.Interface
{
    public interface IUCDGDbContext
    {
       // DbSet<Applications> Applications { get; set; }
         DbSet<Applications> Applications { get; set; }
        DbSet<ApplicationsProjects> ApplicationsProjects { get; set; }
        DbSet<ApplicationStatus> ApplicationStatus { get; set; }
        DbSet<ApplicationSupportRequired> ApplicationSupportRequired { get; set; }
        DbSet<Comments> Comments { get; set; }
        DbSet<CostCentres> CostCentres { get; set; }
        DbSet<Documents> Documents { get; set; }
        DbSet<DocumentSignOffs> DocumentSignOffs { get; set; }
        DbSet<EmailsSent> EmailsSent { get; set; }
        DbSet<ErrorLogs> ErrorLogs { get; set; }
        DbSet<FundingCalls> FundingCalls { get; set; }
        DbSet<FundingCallsProjects> FundingCallsProjects { get; set; }
        DbSet<FundingCallStatus> FundingCallStatus { get; set; }
        DbSet<MessageRecipientDetails> MessageRecipientDetails { get; set; }
        DbSet<Payments> Payments { get; set; }
        DbSet<PersistentSettings> PersistentSettings { get; set; }
        DbSet<ProgressReportComments> ProgressReportComments { get; set; }
        DbSet<ProgressReportDocuments> ProgressReportDocuments { get; set; }
        DbSet<ProgressReports> ProgressReports { get; set; }
        DbSet<ProgressReportStatus> ProgressReportStatus { get; set; }
        DbSet<ProjectCycles> ProjectCycles { get; set; }
        DbSet<Projects> Projects { get; set; }
        DbSet<Qualification> Qualifications { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<SupportRequired> SupportRequired { get; set; }
        DbSet<TemporaryAppointees> TemporaryAppointees { get; set; }
        DbSet<TemporaryApproverApplications> TemporaryApproverApplications { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<UserStoreUser> Users { get; set; }
    }
}
