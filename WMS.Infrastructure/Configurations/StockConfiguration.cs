using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain;

namespace WMS.Infrastructure.Configurations
{
    public class StockConfiguration
        : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("stocks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ComponentId)
                .IsRequired();

            builder.Property(x => x.RackId)
                .IsRequired(); 
            builder.Property(x => x.CellId)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasIndex(x => new { x.ComponentId, x.RackId, x.CellId })
                .IsUnique();

            builder
                .HasOne(x => x.Component)
                .WithMany()
                .HasForeignKey(x => x.ComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Rack)
                .WithMany()
                .HasForeignKey(x => x.RackId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.Cell)
                .WithMany()
                .HasForeignKey(x => x.CellId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
