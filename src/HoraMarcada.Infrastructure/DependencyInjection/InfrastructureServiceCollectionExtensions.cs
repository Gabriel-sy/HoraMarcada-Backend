using HoraMarcada.Application.Common;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Infrastructure.Configuration;
using HoraMarcada.Infrastructure.Notifications;
using HoraMarcada.Infrastructure.Persistence;
using HoraMarcada.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Resend;

namespace HoraMarcada.Infrastructure.DependencyInjection;

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

        services.AddDbContext<HoraMarcadaDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IStoreRepository, EfStoreRepository>();
        services.AddScoped<IProfessionalRepository, EfProfessionalRepository>();
        services.AddScoped<IServiceRepository, EfServiceRepository>();
        services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();
        services.AddScoped<ITimeBlockRepository, EfTimeBlockRepository>();
        services.AddScoped<IAppConfiguration, AppConfiguration>();

        services.AddResend(options =>
        {
            options.ApiToken = configuration["Resend:ApiKey"]!;
        });

        services.AddSingleton<INotificationQueue, ChannelNotificationQueue>();
        services.AddHostedService<NotificationQueueProcessor>();
        services.AddScoped<ResendEmailNotificationService>();
        services.AddScoped<INotificationService, QueueNotificationServiceDecorator>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var supabaseUrl = configuration["Supabase:Url"];
            if (string.IsNullOrWhiteSpace(supabaseUrl))
            {
                throw new InvalidOperationException(
                    "Supabase URL is not configured. Set Supabase:Url in appsettings.");
            }

            var authorityUrl = $"{supabaseUrl.TrimEnd('/')}/auth/v1";
            options.Authority = authorityUrl;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authorityUrl,
                ValidateAudience = true,
                ValidAudience = "authenticated",
                ValidateLifetime = true
            };
        });

        return services;
    }
}

