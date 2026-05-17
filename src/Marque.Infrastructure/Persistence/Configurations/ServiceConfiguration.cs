using Marque.Domain.Entities;
using Marque.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marque.Infrastructure.Persistence.Configurations;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(s => s.Name).HasColumnName("name").IsRequired();
        builder.Property(s => s.DurationMinutes).HasColumnName("duration_min").IsRequired();
        builder.Property(s => s.Price)
            .HasColumnName("price")
            .HasConversion(ValueObjectConverters.NullableMoneyConverter)
            .HasPrecision(10, 2);
        builder.Property(s => s.Active).HasColumnName("active").IsRequired();

        builder.Ignore(s => s.ProfessionalIds);
    }
}
