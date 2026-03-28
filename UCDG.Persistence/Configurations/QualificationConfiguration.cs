using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UCDG.Domain.Entities;

namespace UCDG.Persistence.Configurations
{
    public class QualificationConfiguration : IEntityTypeConfiguration<Qualification>
    {
        public void Configure(EntityTypeBuilder<Qualification> builder)
        {
            builder.Property(o => o.InstitutionName).IsRequired().HasMaxLength(300);
            builder.Property(o => o.Name).IsRequired().HasMaxLength(300);
            builder.Property(o => o.QualificationType).IsRequired().HasMaxLength(200);
        }
    }
}
