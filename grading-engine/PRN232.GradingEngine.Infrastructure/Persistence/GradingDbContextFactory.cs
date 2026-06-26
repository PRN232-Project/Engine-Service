using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PRN232.GradingEngine.Infrastructure.Persistence;

public class GradingDbContextFactory : IDesignTimeDbContextFactory<GradingDbContext>
{
    public GradingDbContext CreateDbContext(string[] args)
    {
        // Look for appsettings.json in the API project first, fallback to current directory
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "PRN232.GradingEngine.Api");
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("SupabaseConnection") 
            ?? "Host=localhost;Database=grading_db;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<GradingDbContext>();
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();
        optionsBuilder.UseNpgsql(dataSource);

        return new GradingDbContext(optionsBuilder.Options);
    }
}
