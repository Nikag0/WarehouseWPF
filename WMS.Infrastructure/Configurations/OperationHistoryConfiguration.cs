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
    public class OperationHistoryConfiguration : IEntityTypeConfiguration<OperationHistory>
    {
        public void Configure(EntityTypeBuilder<OperationHistory> builder)
        {
            builder.ToTable("operationhistory");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.OccurredAt)
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasMaxLength(500);
        }
    }
}
