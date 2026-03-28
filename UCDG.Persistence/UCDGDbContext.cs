using Microsoft.EntityFrameworkCore;
using UCDG.Domain.Entities;
using UCDG.Persistence.Configurations;
using UDCG.Application.Interface;
using static System.Net.Mime.MediaTypeNames;

namespace UCDG.Persistence
{
    public class UCDGDbContext : DbContext
    {
        public UCDGDbContext(DbContextOptions<UCDGDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<UserStoreUser>();

            modelBuilder.Entity<FundingCallsProjects>()
                        .HasKey(t => new { t.FundingCallsId, t.ProjectsId });

            modelBuilder.Entity<ApplicationsProjects>()
                        .HasKey(t => new { t.ApplicationsId, t.ProjectsId });

            modelBuilder.Entity<ApplicationSupportRequired>()
                        .HasKey(t => new { t.ApplicationsId, t.SupportRequiredId });

            modelBuilder.Entity<Applications>(entity =>
            {
                entity.HasOne(a => a.Applicant).WithMany().HasForeignKey(a => a.UserId);

                entity.HasOne(a => a.FundingCalls).WithMany()
                .HasForeignKey(a => a.FundingCallsId);

                entity.HasOne(a => a.ApplicationStatus)
                      .WithMany()
                      .HasForeignKey(a => a.ApplicationStatusId);

                entity.Property(e => e.ApplicantUserStoreUserId)
                      .HasColumnName("ApplicantUserStoreUserId");

                entity.Property(e => e.CurrentApproverStaffNumber)
                      .HasColumnName("CurrentApproverStaffNumber")
                      .HasMaxLength(50);

                entity.Property(e => e.LastApproverRefreshDateUtc)
                      .HasColumnName("LastApproverRefreshDateUtc");

                entity.Property(e => e.LastApproverRefreshByUserStoreUserId)
                      .HasColumnName("LastApproverRefreshByUserStoreUserId");

            });

            modelBuilder.Entity<ApplicationAction>(entity =>
            {
                entity.ToTable("ApplicationActions", "dbo");
                entity.HasKey(x => x.ActionId);

                entity.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
                entity.Property(x => x.ActorStaffNumber).HasMaxLength(50);
                entity.Property(x => x.ApplicantStaffNumber).HasMaxLength(50);
                entity.Property(x => x.ApplicantLineManagerStaffNumberAtAction).HasMaxLength(50);
                entity.Property(x => x.ApplicantDepartmentAtAction).HasMaxLength(200);
                entity.Property(x => x.ApplicantFacultyAtAction).HasMaxLength(200);
                entity.Property(x => x.Comment).HasMaxLength(1000);

                entity.Property(x => x.ActionDateUtc)
                      .HasDefaultValueSql("SYSUTCDATETIME()");
            });

            modelBuilder.Entity<Applications>(entity =>
            {
                entity.Property(x => x.CurrentApproverStaffNumber).HasMaxLength(50);
            });

            modelBuilder.Entity<Applications>()
                .ToTable(tb => tb.HasTrigger("trg_Applications_AfterUpdate"));

            modelBuilder.Entity<Applications>()
                .ToTable(tb => tb.HasTrigger("trg_Applications_AwardLetterResponse"));

        }


        //public DbSet<Role> Roles { get; set; }
        //public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<FundingCalls> FundingCalls { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<FundingCallsProjects> FundingCallsProjects { get; set; }
        public DbSet<ApplicationsProjects> ApplicationsProjects { get; set; }
        //public DbSet<Qualification> Qualifications { get; set; }
        public DbSet<FundingCallStatus> FundingCallStatus { get; set; }
        public DbSet<ProjectCycles> ProjectCycles { get; set; }
        public DbSet<ApplicationStatus> ApplicationStatus { get; set; }
        public DbSet<Applications> Applications { get; set; }
        public DbSet<Documents> Documents { get; set; }
        public DbSet<ProgressReportDocuments> ProgressReportDocuments { get; set; }
        public DbSet<EmailsSent> EmailsSent { get; set; }
        public DbSet<TemporaryAppointees> TemporaryAppointees { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<ProgressReports> ProgressReports { get; set; }
        public DbSet<ProgressReportComments> ProgressReportComments { get; set; }
        public DbSet<ProgressReportStatus> ProgressReportStatus { get; set; }
        public DbSet<PersistentSettings> PersistentSettings { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<CostCentres> CostCentres { get; set; }
        public DbSet<DocumentSignOffs> DocumentSignOffs { get; set; }
        public DbSet<SupportRequired> SupportRequired { get; set; }
        public DbSet<TemporaryApproverApplications> TemporaryApproverApplications { get; set; }
        public DbSet<ApplicationSupportRequired> ApplicationSupportRequired { get; set; }
        public DbSet<ErrorLogs> ErrorLogs { get; set; }

        public DbSet<MessageRecipientDetails> MessageRecipientDetails { get; set; }

        public DbSet<MecMembers> MecMembers { get; set; }
        public DbSet<ApplicationAction> ApplicationActions => Set<ApplicationAction>();
        public DbSet<LinkUserMotivationLetter> LinkUserMotivationLetter { get; set; }
    }
}


