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
            builder.ToTable("operation_history");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.OccurredAt)
                .IsRequired();

            builder.Property(x => x.Comment)
                .HasMaxLength(OperationHistory.CommentMaxLength);

            // numeric(18,3): до трёх знаков после запятой без ошибок округления
            builder.Property(x => x.PreviousQuantity)
                .HasPrecision(QuantityRules.Precision, QuantityRules.Scale);

            builder.Property(x => x.QuantityChanged)
                .HasPrecision(QuantityRules.Precision, QuantityRules.Scale);

            builder.Property(x => x.NewQuantity)
                .HasPrecision(QuantityRules.Precision, QuantityRules.Scale);
        }
    }
}
