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
    public class OperationConfiguration
     : IEntityTypeConfiguration<Operation>
    {
        public void Configure(EntityTypeBuilder<Operation> builder)
        {
            builder.ToTable("operations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.OccurredAt)
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasMaxLength(500);

            builder
                 .HasMany(x => x.Items)
                 .WithOne(x => x.Operation)
                 .HasForeignKey(x => x.OperationId)
                 .OnDelete(DeleteBehavior.Cascade);

            builder
                .Navigation(x => x.Items)
                .HasField("_items");
        }
    }
}
