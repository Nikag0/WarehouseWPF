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

            builder.Property(x => x.CellId)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasIndex(x => new { x.ComponentId, x.CellId })
                .IsUnique();

            builder
                .HasOne<Component>()
                .WithMany()
                .HasForeignKey(x => x.ComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne<Cell>()
                .WithMany()
                .HasForeignKey(x => x.CellId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
