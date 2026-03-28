using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;

namespace UCDG.Persistence.Configurations
{
 public class LinkUserMotivationLetterConfig : IEntityTypeConfiguration<LinkUserMotivationLetter>
    {
        public void Configure(EntityTypeBuilder<LinkUserMotivationLetter> builder)
        {
            builder.ToTable("LinkUserMotivationLetter");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(e => e.FundingCallId)
                   .IsRequired();

            builder.Property(e => e.UserId)
                   .IsRequired();

            builder.Property(e => e.DocumentId)
                   .IsRequired();

            builder.Property(e => e.ApplicationId)
                   .IsRequired(false); // nullable as requested

            // Indexes (tune as needed)
            builder.HasIndex(e => new { e.FundingCallId, e.UserId });
            builder.HasIndex(e => e.DocumentId);
            builder.HasIndex(e => e.ApplicationId);
        }
    }
}
