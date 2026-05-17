using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Marque.Infrastructure.Persistence;

public sealed class MarqueDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MarqueDbContext>
{
    public MarqueDbContext CreateDbContext(string[] args)
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

        var optionsBuilder = new DbContextOptionsBuilder<MarqueDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new MarqueDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectPath()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Marque.Api")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Marque.Api")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "Marque.Api"))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
                return candidate;
        }

        throw new InvalidOperationException("Could not locate Marque.Api appsettings.json for design-time DbContext creation.");
    }
}
