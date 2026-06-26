using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRN232.GradingEngine.Application.Interfaces;
using PRN232.GradingEngine.Infrastructure.Persistence;
using PRN232.GradingEngine.Infrastructure.Persistence.Repositories;
using PRN232.GradingEngine.Infrastructure.StaticAnalysis;
using PRN232.GradingEngine.Infrastructure.TestExecution;

namespace PRN232.GradingEngine.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DB connection
        var connectionString = configuration.GetConnectionString("SupabaseConnection");
        
        bool useLocalFallback = false;
        try
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
            var host = builder.Host;
            if (!string.IsNullOrEmpty(host) && host.Contains("supabase.co"))
            {
                var addressesTask = System.Net.Dns.GetHostAddressesAsync(host);
                if (!addressesTask.Wait(1500)) // 1.5 seconds timeout
                {
                    useLocalFallback = true;
                }
            }
        }
        catch
        {
            useLocalFallback = true;
        }

        if (useLocalFallback)
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var passwords = new[] { "12345", "Thinhtran2412", "postgres", "123456", "admin" };
            string successfulPassword = "postgres";
            
            foreach (var pwd in passwords)
            {
                try
                {
                    var testConnStr = $"Host={dbHost};Database=postgres;Username=postgres;Password={pwd};Timeout=2;CommandTimeout=2;";
                    using (var conn = new Npgsql.NpgsqlConnection(testConnStr))
                    {
                        conn.Open();
                        successfulPassword = pwd;
                        break;
                    }
                }
                catch (Npgsql.PostgresException ex) when (ex.SqlState == "28P01")
                {
                    // Wrong password, continue
                }
                catch (System.Exception)
                {
                    // Other error, just continue
                }
            }

            connectionString = $"Host={dbHost};Database=grading_db;Username=postgres;Password={successfulPassword};Include Error Detail=true;";
            Console.WriteLine($"--> Database Connection: Falling back to local PostgreSQL using password '{successfulPassword}' (Host={dbHost};Database=grading_db).");
        }
        else
        {
            Console.WriteLine("--> Database Connection: Using configured Supabase database.");
        }

        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<GradingDbContext>(options =>
            options.UseNpgsql(dataSource));

        // Repositories
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IExamRubricRepository, ExamRubricRepository>();

        // Analyzers & Builders
        services.AddScoped<IStaticCodeAnalyzer, RoslynStructureAnalyzer>();
        services.AddScoped<ISolutionBuilder, DotnetSolutionBuilder>();

        return services;
    }
}
