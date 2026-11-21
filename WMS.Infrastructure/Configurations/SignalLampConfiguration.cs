using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.LedStrip;

namespace WMS.Infrastructure.Configurations
{
    public class SignalLampConfiguration : IEntityTypeConfiguration<SignalLamp>
    {
        public void Configure(EntityTypeBuilder<SignalLamp> builder)
        {
            builder.ToTable("signal_lamp");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Name)
                   .IsRequired()
                   .HasMaxLength(SignalLamp.NameMaxLength);

            // Контроллер, к GPIO которого подключена лампа. Пока лампа существует, контроллер удалить нельзя.
            builder.HasOne(l => l.Microcontroller)
                   .WithMany()
                   .HasForeignKey(l => l.MicrocontrollerId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Контроллеры, которые сигнализируют этой лампой. При удалении лампы у них просто пропадает привязка.
            builder.HasMany(l => l.AssignedControllers)
                   .WithOne(m => m.SignalLamp)
                   .HasForeignKey(m => m.SignalLampId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
