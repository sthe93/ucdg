using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UCDG.Domain.Entities;

namespace UCDG.Persistence
{
    public class UserStoreDbContext : DbContext
    {
        public UserStoreDbContext(DbContextOptions<UserStoreDbContext> options) : base(options)
        {
        }

        public DbSet<UserStoreUser> Users { get; set; }
        public DbSet<TemporaryUserRole> TemporaryUserRoles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Qualification> Qualifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // USERS
            modelBuilder.Entity<UserStoreUser>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.Username).HasColumnName("Username").HasMaxLength(200);
                entity.Property(e => e.HRPostNumber).HasColumnName("HRPostNumber").HasMaxLength(50);

                entity.Property(e => e.IsActive).HasColumnName("IsActive");
                entity.Property(e => e.IsLocked).HasColumnName("IsLocked");

                entity.Property(e => e.LineManagerStaffNumber).HasColumnName("LineManagerStaffNumber").HasMaxLength(50);
                entity.Property(e => e.FacultyDivision).HasColumnName("FacultyDivision").HasMaxLength(200);
                entity.Property(e => e.Department).HasColumnName("Department").HasMaxLength(200);
                entity.Property(e => e.ViceDeanStaffNumber).HasColumnName("ViceDeanStaffNumber").HasMaxLength(50);

            });

            modelBuilder.Entity<TemporaryUserRole>(entity =>
            {
                entity.ToTable("TemporaryUserRoles");

                entity.HasKey(e => e.TemporaryUserRoleId);

                entity.Property(e => e.TemporaryUserRoleId).HasColumnName("TemporaryUserRoleId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.RoleId).HasColumnName("RoleId");
                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationId");

                entity.Property(e => e.CreatedDate).HasColumnName("CreatedDate");
                entity.Property(e => e.CreatedBy).HasColumnName("CreatedBy");
                entity.Property(e => e.ModifiedBy).HasColumnName("ModifiedBy");
                entity.Property(e => e.ModifiedDate).HasColumnName("ModifiedDate");

                entity.Property(e => e.IsActive).HasColumnName("IsActive");

                entity.HasOne(t => t.User).WithMany(u => u.TemporaryUserRoles).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.NoAction);

            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");

                entity.HasKey(e => e.RoleId);

                entity.Property(e => e.RoleId).HasColumnName("RoleId");
                entity.Property(e => e.AppId).HasColumnName("AppId");

                entity.Property(e => e.RoleType).HasColumnName("RoleType").HasMaxLength(200);

                entity.Property(e => e.IsActive).HasColumnName("IsActive");
                entity.Property(e => e.IsTemporary).HasColumnName("IsTemporary");
                entity.Property(e => e.IsAssignable).HasColumnName("IsAssignable");

                entity.Ignore(e => e.Name);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");

                entity.HasKey(e => e.UserRoleId);

                entity.Property(e => e.UserRoleId).HasColumnName("UserRoleId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.RoleId).HasColumnName("RoleId");

                entity.Property(e => e.IsActive).HasColumnName("IsActive");

                entity.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.NoAction);

                entity.Ignore(e => e.ExpiryDate);
                entity.Ignore(e => e.CreatedBy);
                entity.Ignore(e => e.DateCreated);
                entity.Ignore(e => e.ModifiedBy);
                entity.Ignore(e => e.DateModified);

            });
            modelBuilder.Entity<Qualification>(entity =>
            {
                entity.ToTable("Qualifications");
                entity.HasKey(q => q.QualificationId);

                entity.Property(q => q.QualificationId).HasColumnName("QualificationId");
                entity.Property(q => q.UserId).HasColumnName("UserId");

                entity.HasOne(q => q.User)
                      .WithMany(u => u.Qualifications)
                      .HasForeignKey(q => q.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

        }
    }
}

