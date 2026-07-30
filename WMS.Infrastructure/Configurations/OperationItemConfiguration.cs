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
    public class OperationItemConfiguration
     : IEntityTypeConfiguration<HistoryItem>
    {
        public void Configure(EntityTypeBuilder<HistoryItem> builder)
        {
            builder.ToTable("operation_items");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ComponentId)
                .IsRequired();

            builder.Property(x => x.RackId)
                .IsRequired();

            builder.Property(x => x.CellId)
                .IsRequired();

            builder.Property(x => x.QuantityBefore)
                .IsRequired();

            builder.Property(x => x.QuantityAfter)
                .IsRequired();
        }
    }
}
