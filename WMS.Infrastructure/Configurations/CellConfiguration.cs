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

            builder.Property(x => x.Row).IsRequired();
            builder.Property(x => x.Rack).IsRequired();
            builder.Property(x => x.Line).IsRequired();
            builder.Property(x => x.Column).IsRequired();

            builder.HasIndex(x => new { x.Row, x.Rack, x.Line, x.Column })
                .IsUnique();
        }
    }
}
