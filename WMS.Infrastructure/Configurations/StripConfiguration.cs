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
    public class StripConfiguration : IEntityTypeConfiguration<Strip>
    {

        public void Configure(EntityTypeBuilder<Strip> builder)
        {
            builder.ToTable("strip");

            builder.HasKey(s => s.Id);

            builder.Property(m => m.StripNumber)
                   .HasColumnType("smallint")
                   .HasConversion<byte>();

            builder.HasIndex(s => new { s.MicrocontrollerId, s.StripNumber })
                   .IsUnique();

            builder.HasOne(s => s.Microcontroller)
                   .WithMany(m => m.Strips)
                   .HasForeignKey(s => s.MicrocontrollerId);

            builder.HasMany(s => s.Sectors)
                   .WithOne(se => se.Strip)
                   .HasForeignKey(se => se.StripId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
