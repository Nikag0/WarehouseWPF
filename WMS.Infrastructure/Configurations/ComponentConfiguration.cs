using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using WMS.Domain;

namespace WMS.Infrastructure.Configurations
{
    public class ComponentConfiguration: IEntityTypeConfiguration<Component>
    {
        public void Configure(EntityTypeBuilder<Component> builder)
        {
            builder.ToTable("components");

            builder.HasQueryFilter(c => !c.IsDeleted);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Article)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.Article)
                .IsUnique();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Manufacturer)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.MinQuantity)
                .IsRequired();
        }
    }
}
