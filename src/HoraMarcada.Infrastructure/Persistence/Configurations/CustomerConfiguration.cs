using HoraMarcada.Domain.Entities;
using HoraMarcada.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoraMarcada.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(c => c.Name).HasColumnName("name").IsRequired();
        builder.Property(c => c.WhatsApp)
            .HasColumnName("whatsapp")
            .HasConversion(ValueObjectConverters.PhoneNumberConverter)
            .IsRequired();
        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasConversion(ValueObjectConverters.NullableEmailConverter);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(c => c.StoreId).HasDatabaseName("idx_customers_store");
        builder.HasIndex(c => new { c.StoreId, c.WhatsApp }).IsUnique().HasDatabaseName("idx_customers_store_whatsapp");

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
