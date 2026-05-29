using HoraMarcada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoraMarcada.Infrastructure.Persistence.Configurations;

internal sealed class TimeBlockConfiguration : IEntityTypeConfiguration<TimeBlock>
{
    public void Configure(EntityTypeBuilder<TimeBlock> builder)
    {
        builder.ToTable("time_blocks");

        builder.HasKey(tb => tb.Id);
        builder.Property(tb => tb.Id).HasColumnName("id");
        builder.Property(tb => tb.StoreId).HasColumnName("store_id").IsRequired();
        builder.Property(tb => tb.ProfessionalId).HasColumnName("professional_id").IsRequired();
        builder.Property(tb => tb.StartTime).HasColumnName("start_time").IsRequired();
        builder.Property(tb => tb.EndTime).HasColumnName("end_time").IsRequired();
        builder.Property(tb => tb.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(tb => new { tb.ProfessionalId, tb.StartTime, tb.EndTime })
            .HasDatabaseName("idx_time_blocks_professional_start");

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(tb => tb.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Professional>()
            .WithMany()
            .HasForeignKey(tb => tb.ProfessionalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
