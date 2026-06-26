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
