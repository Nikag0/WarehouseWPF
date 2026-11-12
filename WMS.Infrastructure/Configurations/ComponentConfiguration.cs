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
            builder.ToTable("component");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Article)
                .IsRequired()
                .HasMaxLength(Component.TextMaxLength);

            builder.HasIndex(c => c.Article)
                .IsUnique();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(Component.TextMaxLength);

            builder.Property(x => x.Manufacturer)
                .IsRequired()
                .HasMaxLength(Component.TextMaxLength);

            builder.Property(x => x.MinQuantity)
                .IsRequired();
        }
    }
}
