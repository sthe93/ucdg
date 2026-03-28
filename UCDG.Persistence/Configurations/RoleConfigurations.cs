using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using UCDG.Domain.Entities;

namespace UCDG.Persistence.Configurations
{
    public class RoleConfigurations : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(o => o.Name).IsRequired();
            //there is more properties you can set like builder.Property(q => q.ActualQuestion).HasMaxLength(5000);
            /*.HasMaxLength(50);*/
        }
    }
}
