using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain.LedStrip;

namespace WMS.Infrastructure.Configurations
{
    public class SectorConfiguration : IEntityTypeConfiguration<Sector>
    {
        public void Configure(EntityTypeBuilder<Sector> builder)
        {
            builder.ToTable("sector");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Index)
            .IsRequired()
            .HasColumnType("smallint")
            .HasConversion<byte>();

            builder.Property(s => s.StartDiode).IsRequired();
            builder.Property(s => s.EndDiode).IsRequired();

            builder.Property(s => s.R)
                   .HasColumnType("smallint")
                   .HasConversion<byte>();

            builder.Property(s => s.G)
                   .HasColumnType("smallint")
                   .HasConversion<byte>();

            builder.Property(s => s.B)
                   .HasColumnType("smallint")
                   .HasConversion<byte>();

            builder.Property(s => s.Bright)
                   .HasColumnType("smallint")
                   .HasConversion<byte>();

            builder.HasIndex(s => new { s.StripId, s.Index })
                   .IsUnique();

            builder.HasIndex(s => s.CellId)
                   .IsUnique();

            // ИСПРАВЛЕНО: двойные кавычки для PostgreSQL
            builder.HasCheckConstraint("CK_Sector_Range", "\"StartDiode\" <= \"EndDiode\"");

            builder.HasOne(s => s.Strip)
                   .WithMany(st => st.Sectors)
                   .HasForeignKey(s => s.StripId);

            builder.HasOne(s => s.Cell)
                   .WithMany()
                   .HasForeignKey(s => s.CellId);
            
        }
    }
}
