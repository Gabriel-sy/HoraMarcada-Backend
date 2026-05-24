using HoraMarcada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoraMarcada.Infrastructure.Persistence;

public sealed class HoraMarcadaDbContext(DbContextOptions<HoraMarcadaDbContext> options) : DbContext(options)
{
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<WeeklyHours> WeeklyHours => Set<WeeklyHours>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceProfessional> ServiceProfessionals => Set<ServiceProfessional>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HoraMarcadaDbContext).Assembly);
    }
}
