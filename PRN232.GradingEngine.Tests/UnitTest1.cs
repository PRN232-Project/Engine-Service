using System.Text;
using PRN232.Domain.Entities;
using PRN232.GradingEngine.Infrastructure.StaticAnalysis;

namespace PRN232.GradingEngine.Tests;

public class RoslynStructureAnalyzerTests : IDisposable
{
    private readonly string _workspaceRoot;
    private readonly RoslynStructureAnalyzer _sut = new();

    public RoslynStructureAnalyzerTests()
    {
        _workspaceRoot = Path.Combine(Path.GetTempPath(), "prn232-band0-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workspaceRoot);
    }

    [Fact]
    public async Task CheckNamingConventionsAsync_ShouldPass_WhenRequiredSolutionProjectAndFileExist()
    {
        var workspace = CreateWorkspace("SE182004");
        var studentId = "SE182004";

        // solution + projects
        WriteFile(Path.Combine(workspace, "PRN231_SU25_SE182004.sln"), BuildSampleSlnContent());
        CreateProject(workspace, "PRN231_SU25_SE182004.API");
        CreateProject(workspace, "PRN231_SU25_SE182004.Business");
        CreateProject(workspace, "PRN231_SU25_SE182004.Repository");

        // required file
        WriteFile(Path.Combine(workspace, "PRN231_SU25_SE182004.postman_collection.json"), "{}");

        var rubric = BuildRubric();

        var violations = await _sut.CheckNamingConventionsAsync(workspace, studentId, rubric);

        Assert.Empty(violations);
    }

    [Fact]
    public async Task CheckNamingConventionsAsync_ShouldReportViolation_WhenRequiredProjectMissing()
    {
        var workspace = CreateWorkspace("SE182004");
        var studentId = "SE182004";

        WriteFile(Path.Combine(workspace, "PRN231_SU25_SE182004.sln"), BuildSampleSlnContent());
        CreateProject(workspace, "PRN231_SU25_SE182004.API");
        CreateProject(workspace, "PRN231_SU25_SE182004.Repository");
        // Missing Business project intentionally

        WriteFile(Path.Combine(workspace, "PRN231_SU25_SE182004.postman_collection.json"), "{}");

        var rubric = BuildRubric();

        var violations = await _sut.CheckNamingConventionsAsync(workspace, studentId, rubric);

        Assert.Contains(violations, v => v.Contains("Business", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckHardcodedConnectionStringAsync_ShouldReportMissingConnectionStrings_WhenAppsettingsInvalid()
    {
        var workspace = CreateWorkspace("SE182004");
        Directory.CreateDirectory(Path.Combine(workspace, "PRN231_SU25_SE182004.API"));

        WriteFile(
            Path.Combine(workspace, "PRN231_SU25_SE182004.API", "appsettings.json"),
            "{ \"Logging\": { \"LogLevel\": { \"Default\": \"Information\" } } }");

        var violations = await _sut.CheckHardcodedConnectionStringAsync(workspace);

        Assert.Contains(violations, v => v.Contains("ConnectionStrings", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckHardcodedConnectionStringAsync_ShouldReportHardcodedLiteral_InOnConfiguring()
    {
        var workspace = CreateWorkspace("SE182004");
        var repoDir = Path.Combine(workspace, "PRN231_SU25_SE182004.Repository");
        Directory.CreateDirectory(repoDir);

        WriteFile(
            Path.Combine(workspace, "appsettings.json"),
            "{ \"ConnectionStrings\": { \"Default\": \"Server=.;Database=PRN232;User Id=sa;Password=123\" } }");

        WriteFile(
            Path.Combine(repoDir, "AppDbContext.cs"),
            """
            using Microsoft.EntityFrameworkCore;

            public class AppDbContext : DbContext
            {
                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                {
                    optionsBuilder.UseSqlServer("Server=.;Database=PRN232;User Id=sa;Password=123");
                }
            }
            """);

        var violations = await _sut.CheckHardcodedConnectionStringAsync(workspace);

        Assert.Contains(violations, v => v.Contains("hardcode Connection String", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CheckHardcodedConnectionStringAsync_ShouldIgnoreHardcodedLiteral_InMigrationsFolder()
    {
        var workspace = CreateWorkspace("SE182004");
        var migrationDir = Path.Combine(workspace, "PRN231_SU25_SE182004.Repository", "Migrations");
        Directory.CreateDirectory(migrationDir);

        WriteFile(
            Path.Combine(workspace, "appsettings.json"),
            "{ \"ConnectionStrings\": { \"Default\": \"Server=.;Database=PRN232;User Id=sa;Password=123\" } }");

        WriteFile(
            Path.Combine(migrationDir, "Snapshot.cs"),
            """
            using Microsoft.EntityFrameworkCore;

            public class Snapshot
            {
                protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                {
                    optionsBuilder.UseSqlServer("Server=.;Database=PRN232;User Id=sa;Password=123");
                }
            }
            """);

        var violations = await _sut.CheckHardcodedConnectionStringAsync(workspace);

        Assert.DoesNotContain(violations, v => v.Contains("Snapshot.cs", StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        if (Directory.Exists(_workspaceRoot))
        {
            Directory.Delete(_workspaceRoot, recursive: true);
        }
    }

    private static ExamRubric BuildRubric()
    {
        return new ExamRubric
        {
            SolutionPattern = @"PRN231_SU25_{StudentID}",
            RequiredProjects =
            [
                new RequiredProject { Pattern = @"PRN231_SU25_{StudentID}\.API", MustExist = true },
                new RequiredProject { Pattern = @"PRN231_SU25_{StudentID}\.Business", MustExist = true },
                new RequiredProject { Pattern = @"PRN231_SU25_{StudentID}\.Repository", MustExist = true }
            ],
            RequiredFiles =
            [
                new RequiredFile { Pattern = @"PRN231_SU25_{StudentID}\.postman_collection\.json", MustExist = true }
            ]
        };
    }

    private string CreateWorkspace(string studentId)
    {
        var ws = Path.Combine(_workspaceRoot, $"submission-{studentId}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(ws);
        return ws;
    }

    private static void CreateProject(string workspace, string projectName)
    {
        var projectDir = Path.Combine(workspace, projectName);
        Directory.CreateDirectory(projectDir);
        WriteFile(Path.Combine(projectDir, $"{projectName}.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");
    }

    private static string BuildSampleSlnContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");
        sb.AppendLine("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"PRN231_SU25_SE182004.API\", \"PRN231_SU25_SE182004.API\\PRN231_SU25_SE182004.API.csproj\", \"{11111111-1111-1111-1111-111111111111}\"");
        sb.AppendLine("EndProject");
        sb.AppendLine("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"PRN231_SU25_SE182004.Business\", \"PRN231_SU25_SE182004.Business\\PRN231_SU25_SE182004.Business.csproj\", \"{22222222-2222-2222-2222-222222222222}\"");
        sb.AppendLine("EndProject");
        sb.AppendLine("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"PRN231_SU25_SE182004.Repository\", \"PRN231_SU25_SE182004.Repository\\PRN231_SU25_SE182004.Repository.csproj\", \"{33333333-3333-3333-3333-333333333333}\"");
        sb.AppendLine("EndProject");
        return sb.ToString();
    }

    private static void WriteFile(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }
}
