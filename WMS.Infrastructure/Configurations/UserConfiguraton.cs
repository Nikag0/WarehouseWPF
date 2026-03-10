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
    public class UserConfiguraton : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Surname).IsRequired();
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Patronymic).IsRequired();

            builder.HasIndex(x => new { x.Surname, x.Name, x.Patronymic })
                .IsUnique();
        }
    }
}
