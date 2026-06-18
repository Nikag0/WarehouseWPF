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
    public class RackConfiguration : IEntityTypeConfiguration<Rack>
    {
        public void Configure(EntityTypeBuilder<Rack> builder)
        {
            builder.ToTable("racks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Column)
                .IsRequired();

            builder.Property(x => x.Row)
                .IsRequired();

            builder.Property(x => x.Type)
               .IsRequired();

            builder.HasIndex(x => new { x.Column, x.Row })
                .IsUnique();

            // Связь (опционально, но полезно явно задать)
            builder.HasMany(x => x.Cells)
                .WithOne(c => c.Rack)
                .HasForeignKey(c => c.RackId);
        }
    }
}
