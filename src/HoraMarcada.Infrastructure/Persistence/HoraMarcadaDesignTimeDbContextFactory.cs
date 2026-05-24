using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HoraMarcada.Infrastructure.Persistence;

public sealed class HoraMarcadaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<HoraMarcadaDbContext>
{
    public HoraMarcadaDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveApiProjectPath();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Supabase");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'Supabase' is not configured. Set ConnectionStrings:Supabase in appsettings or ConnectionStrings__Supabase environment variable.");

        var optionsBuilder = new DbContextOptionsBuilder<HoraMarcadaDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new HoraMarcadaDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectPath()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "HoraMarcada.Api")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "HoraMarcada.Api")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "HoraMarcada.Api"))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
                return candidate;
        }

        throw new InvalidOperationException("Could not locate HoraMarcada.Api appsettings.json for design-time DbContext creation.");
    }
}
