using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Infrastructure.Configurations
{
    public class OperatorConfiguraton : IEntityTypeConfiguration<Operator>
    {
        public void Configure(EntityTypeBuilder<Operator> builder)
        {
            builder.ToTable("operators");

            builder.HasQueryFilter(o => !o.IsDeleted);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Surname).IsRequired();
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Patronymic).IsRequired();

            builder.HasIndex(x => new { x.Surname, x.Name, x.Patronymic })
                .IsUnique();
        }
    }
}
