using Marque.Application.Repositories;
using Marque.Infrastructure.Persistence;
using Marque.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marque.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Supabase");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Supabase' is not configured. Set ConnectionStrings:Supabase in appsettings or ConnectionStrings__Supabase environment variable.");
        }

        services.AddDbContext<MarqueDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IStoreRepository, EfStoreRepository>();
        services.AddScoped<IProfessionalRepository, EfProfessionalRepository>();
        services.AddScoped<IServiceRepository, EfServiceRepository>();
        services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();

        return services;
    }
}
