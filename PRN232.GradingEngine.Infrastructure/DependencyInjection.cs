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
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine("--> Database Connection: Using configured Supabase database.");

        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<GradingDbContext>(options =>
            options.UseNpgsql(dataSource, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "grading")));

        // Repositories
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IExamRubricRepository, ExamRubricRepository>();

        // Analyzers & Builders & Test Runners
        services.AddScoped<IStaticCodeAnalyzer, RoslynStructureAnalyzer>();
        services.AddScoped<ISolutionBuilder, DotnetSolutionBuilder>();
        services.AddScoped<ITestRunner, DynamicApiTestRunner>();

        // gRPC Client & Security Services
        services.AddGrpcClient<PRN232.Common.Grpc.AuthGrpcService.AuthGrpcServiceClient>(options =>
        {
            var url = configuration["GrpcSettings:ExamAccountUrl"] ?? "http://localhost:5178";
            options.Address = new Uri(url);
        });
        services.AddScoped<IAuthService, Security.GrpcAuthService>();

        return services;
    }
}
