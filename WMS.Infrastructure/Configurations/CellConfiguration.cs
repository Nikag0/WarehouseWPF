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
    public class CellConfiguration : IEntityTypeConfiguration<Cell>
    {
        public void Configure(EntityTypeBuilder<Cell> builder)
        {
            builder.ToTable("cells");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RackId)
                .IsRequired();

            builder.Property(x => x.Line)
                .IsRequired();

            builder.Property(x => x.Column)
                .IsRequired();

            // Связь: один Rack -> много Cell
            builder.HasOne(x => x.Rack)
                .WithMany(r => r.Cells)
                .HasForeignKey(x => x.RackId)
                .OnDelete(DeleteBehavior.Cascade);

            // Уникальность внутри одного Rack
            builder.HasIndex(x => new { x.RackId, x.Line, x.Column })
                .IsUnique();
        }
    }
}
