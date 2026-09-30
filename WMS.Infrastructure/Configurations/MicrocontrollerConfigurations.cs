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
    public class MicrocontrollerConfiguration : IEntityTypeConfiguration<Microcontroller>
    {
        public void Configure(EntityTypeBuilder<Microcontroller> builder)
        {
            builder.ToTable("microcontroller");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Ip)
                   .IsRequired()
                   .HasMaxLength(15);

            builder.Property(m => m.Port)
                   .IsRequired();

            builder.Property(m => m.DeviceAddress)
                   .HasColumnType("smallint")
                   .HasConversion<byte>();

            builder.HasMany(m => m.Strips)
                   .WithOne(s => s.Microcontroller)
                   .HasForeignKey(s => s.MicrocontrollerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
