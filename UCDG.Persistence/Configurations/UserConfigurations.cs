using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UCDG.Domain.Entities;

namespace UCDG.Persistence.Configurations
{

    public class UserConfigurations : IEntityTypeConfiguration<UserStoreUser>
    {
        public void Configure(EntityTypeBuilder<UserStoreUser> builder)
        {
            
            builder.Property(o => o.UserId);
            builder.Property(o => o.Username).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Name).IsRequired().HasMaxLength(150);
            builder.Property(o => o.Surname).IsRequired().HasMaxLength(150);
            builder.Property(o => o.Nationality).IsRequired().HasMaxLength(50);
            builder.Property(o => o.IdNumber).IsRequired().HasMaxLength(50);
            builder.Property(o => o.DateOfBirth).IsRequired().HasMaxLength(25);
            builder.Property(o => o.Email).IsRequired().HasMaxLength(50);
            builder.Property(o => o.AlternativeEmailAddress).IsRequired().HasMaxLength(50);
            builder.Property(o => o.CellPhone).HasMaxLength(25);
            //builder.Property(o => o.TelephoneNumber).HasMaxLength(25);
            builder.Property(o => o.CreatedDate).IsRequired();
            builder.Property(o => o.ModifiedDate).IsRequired();
            builder.Property(o => o.IsProfileCompleted).IsRequired();
            //builder.Property(o => o.OtherProgramme).IsRequired();
            //builder.Property(o => o.OtherProgrammeName).HasMaxLength(200);
            //builder.Property(o => o.OtherFundSource).IsRequired();
            //builder.Property(o => o.OtherFundSourceName).HasMaxLength(200);
        }
    }
}
