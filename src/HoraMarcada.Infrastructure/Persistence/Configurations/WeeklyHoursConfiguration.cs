using HoraMarcada.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoraMarcada.Infrastructure.Persistence.Configurations;

internal sealed class WeeklyHoursConfiguration : IEntityTypeConfiguration<WeeklyHours>
{
    public void Configure(EntityTypeBuilder<WeeklyHours> builder)
    {
        builder.ToTable("weekly_hours");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.ProfessionalId).HasColumnName("professional_id").IsRequired();
        builder.Property(w => w.DayOfWeek).HasColumnName("day_of_week").IsRequired();
        builder.Property(w => w.StartTime).HasColumnName("start_time").IsRequired();
        builder.Property(w => w.EndTime).HasColumnName("end_time").IsRequired();
        builder.Property(w => w.LunchStart).HasColumnName("lunch_start");
        builder.Property(w => w.LunchEnd).HasColumnName("lunch_end");
        builder.Property(w => w.IntervalMinutes).HasColumnName("interval_min").IsRequired();

        builder.HasIndex(w => new { w.ProfessionalId, w.DayOfWeek }).IsUnique();
    }
}
