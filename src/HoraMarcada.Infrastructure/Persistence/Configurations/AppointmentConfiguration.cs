using HoraMarcada.Domain.Entities;
using HoraMarcada.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoraMarcada.Infrastructure.Persistence.Configurations;

internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments", t =>
            t.HasCheckConstraint(
                "CK_appointments_status",
                "status IN ('confirmed', 'canceled', 'completed')"));

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(a => a.ProfessionalId).HasColumnName("professional_id").IsRequired();
        builder.Property(a => a.ServiceId).HasColumnName("service_id").IsRequired();
        builder.Property(a => a.CustomerName).HasColumnName("customer_name").IsRequired();
        builder.Property(a => a.CustomerWhatsApp)
            .HasColumnName("customer_whatsapp")
            .HasConversion(ValueObjectConverters.PhoneNumberConverter)
            .IsRequired();
        builder.Property(a => a.CustomerEmail)
            .HasColumnName("customer_email")
            .HasConversion(ValueObjectConverters.EmailConverter)
            .IsRequired();
        builder.Property(a => a.StartTime).HasColumnName("start_time").IsRequired();
        builder.Property(a => a.EndTime).HasColumnName("end_time").IsRequired();
        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion(ValueObjectConverters.AppointmentStatusConverter)
            .IsRequired();
        builder.Property(a => a.CancelToken).HasColumnName("cancel_token");
        builder.HasIndex(a => a.CancelToken).IsUnique();
        builder.Property(a => a.CancelTokenUsed).HasColumnName("cancel_token_used");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(a => new { a.ProfessionalId, a.StartTime })
            .HasDatabaseName("idx_appointments_professional_start");
        builder.HasIndex(a => new { a.StoreId, a.StartTime })
            .HasDatabaseName("idx_appointments_store_start");

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(a => a.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Professional>()
            .WithMany()
            .HasForeignKey(a => a.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Service>()
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
