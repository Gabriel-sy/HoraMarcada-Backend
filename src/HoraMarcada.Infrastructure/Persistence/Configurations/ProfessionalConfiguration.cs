using HoraMarcada.Domain.Entities;
using HoraMarcada.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoraMarcada.Infrastructure.Persistence.Configurations;

internal sealed class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable("professionals");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").IsRequired();
        builder.Property(p => p.PhotoUrl).HasColumnName("photo_url");
        builder.Property(p => p.WhatsApp)
            .HasColumnName("whatsapp")
            .HasConversion(ValueObjectConverters.NullablePhoneNumberConverter);
        builder.Property(p => p.Email)
            .HasColumnName("email")
            .HasConversion(ValueObjectConverters.NullableEmailConverter);
        builder.Property(p => p.Active).HasColumnName("active").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();

        var weeklyHoursNavigation = builder.Metadata.FindNavigation(nameof(Professional.WeeklySchedules))!;
        weeklyHoursNavigation.SetField("_weeklyHours");
        weeklyHoursNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(p => p.WeeklySchedules)
            .WithOne()
            .HasForeignKey(w => w.ProfessionalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
