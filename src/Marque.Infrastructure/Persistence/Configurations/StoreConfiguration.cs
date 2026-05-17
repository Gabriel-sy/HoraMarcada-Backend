using Marque.Domain.Entities;
using Marque.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marque.Infrastructure.Persistence.Configurations;

internal sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("stores");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.Name).HasColumnName("name").IsRequired();
        builder.Property(s => s.Slug)
            .HasColumnName("slug")
            .HasConversion(ValueObjectConverters.SlugConverter)
            .IsRequired();
        builder.HasIndex(s => s.Slug).IsUnique();
        builder.Property(s => s.LogoUrl).HasColumnName("logo_url");
        builder.Property(s => s.Address).HasColumnName("address");
        builder.Property(s => s.AddressLat).HasColumnName("address_lat").HasPrecision(10, 7);
        builder.Property(s => s.AddressLng).HasColumnName("address_lng").HasPrecision(10, 7);
        builder.Property(s => s.WhatsApp)
            .HasColumnName("whatsapp")
            .HasConversion(ValueObjectConverters.PhoneNumberConverter)
            .IsRequired();
        builder.Property(s => s.Active).HasColumnName("active").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();

        var professionalsNavigation = builder.Metadata.FindNavigation(nameof(Store.Professionals))!;
        professionalsNavigation.SetField("_professionals");
        professionalsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(s => s.Professionals)
            .WithOne()
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        var servicesNavigation = builder.Metadata.FindNavigation(nameof(Store.Services))!;
        servicesNavigation.SetField("_services");
        servicesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(s => s.Services)
            .WithOne()
            .HasForeignKey(s => s.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
