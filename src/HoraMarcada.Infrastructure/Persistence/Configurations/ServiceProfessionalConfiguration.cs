using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoraMarcada.Infrastructure.Persistence.Configurations;

internal sealed class ServiceProfessionalConfiguration : IEntityTypeConfiguration<ServiceProfessional>
{
    public void Configure(EntityTypeBuilder<ServiceProfessional> builder)
    {
        builder.ToTable("service_professional");

        builder.HasKey(sp => new { sp.ServiceId, sp.ProfessionalId });
        builder.Property(sp => sp.ServiceId).HasColumnName("service_id");
        builder.Property(sp => sp.ProfessionalId).HasColumnName("professional_id");

        builder.HasOne<HoraMarcada.Domain.Entities.Service>()
            .WithMany()
            .HasForeignKey(sp => sp.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<HoraMarcada.Domain.Entities.Professional>()
            .WithMany()
            .HasForeignKey(sp => sp.ProfessionalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
