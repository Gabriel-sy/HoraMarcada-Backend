using Marque.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marque.Infrastructure.Persistence;

public sealed class MarqueDbContext(DbContextOptions<MarqueDbContext> options) : DbContext(options)
{
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<WeeklyHours> WeeklyHours => Set<WeeklyHours>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceProfessional> ServiceProfessionals => Set<ServiceProfessional>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarqueDbContext).Assembly);
    }
}
