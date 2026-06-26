using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using PRN232.Domain.ValueObjects;
using PRN232.Domain.Entities;
using PRN232.GradingEngine.Infrastructure.StaticAnalysis;

namespace PRN232.GradingEngine.Tests;

public class RoslynStructureAnalyzerTests : IDisposable
{
    private readonly string _tempWorkspace;
    private readonly RoslynStructureAnalyzer _analyzer;

    public RoslynStructureAnalyzerTests()
    {
        _tempWorkspace = Path.Combine(Directory.GetCurrentDirectory(), "temp_workspace_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempWorkspace);
        _analyzer = new RoslynStructureAnalyzer();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWorkspace))
        {
            Directory.Delete(_tempWorkspace, true);
        }
    }

    [Fact]
    public async Task CheckNamingConventions_ValidStructure_ReturnsNoViolations()
    {
        // Arrange
        var studentId = "SE182004";
        
        // Write mock solution file
        var slnPath = Path.Combine(_tempWorkspace, $"PRN231_SU25_SE{studentId}.sln");
        var slnContent = $@"
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""PRN231_SU25_SE{studentId}.api"", ""PRN231_SU25_SE{studentId}.api\PRN231_SU25_SE{studentId}.api.csproj"", ""{{87C3946E-AA41-477D-A08D-75DCDCD7EC8E}}""
EndProject
";
        await File.WriteAllTextAsync(slnPath, slnContent);

        // Write mock project directory and csproj file
        var projDir = Path.Combine(_tempWorkspace, $"PRN231_SU25_SE{studentId}.api");
        Directory.CreateDirectory(projDir);
        await File.WriteAllTextAsync(Path.Combine(projDir, $"PRN231_SU25_SE{studentId}.api.csproj"), "<Project></Project>");

        // Write mock postman json script
        await File.WriteAllTextAsync(Path.Combine(_tempWorkspace, $"PRN231_SU25_SE{studentId}.json"), "{}");

        var rubric = new ExamRubric
        {
            SolutionPattern = "^PRN231_SU25_SE{StudentID}$",
            RequiredProjects = new List<RequiredProject>
            {
                new() { Pattern = "^PRN231_SU25_SE{StudentID}\\.api$", MustExist = true }
            },
            RequiredFiles = new List<RequiredFile>
            {
                new() { Pattern = "^PRN231_SU25_SE{StudentID}\\.json$", MustExist = true }
            }
        };

        // Act
        var violations = await _analyzer.CheckNamingConventionsAsync(_tempWorkspace, studentId, rubric);

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task CheckNamingConventions_InvalidStructure_ReturnsViolations()
    {
        // Arrange
        var studentId = "SE182004";
        
        // Write mismatched solution file
        var slnPath = Path.Combine(_tempWorkspace, "WrongSolutionName.sln");
        await File.WriteAllTextAsync(slnPath, "Project(...) = ...");

        var rubric = new ExamRubric
        {
            SolutionPattern = "^PRN231_SU25_SE{StudentID}$",
            RequiredProjects = new List<RequiredProject>
            {
                new() { Pattern = "^PRN231_SU25_SE{StudentID}\\.api$", MustExist = true }
            },
            RequiredFiles = new List<RequiredFile>
            {
                new() { Pattern = "^PRN231_SU25_SE{StudentID}\\.json$", MustExist = true }
            }
        };

        // Act
        var violations = await _analyzer.CheckNamingConventionsAsync(_tempWorkspace, studentId, rubric);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(violations, v => v.Contains("không đúng cấu trúc bắt buộc"));
        Assert.Contains(violations, v => v.Contains("Không tìm thấy project bắt buộc"));
        Assert.Contains(violations, v => v.Contains("Không tìm thấy file bắt buộc"));
    }

    [Fact]
    public async Task CheckHardcodedConnectionString_DetectsViolations()
    {
        // Arrange
        var csPath = Path.Combine(_tempWorkspace, "DbContext.cs");
        var badCode = @"
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(""Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"");
    }
}
";
        await File.WriteAllTextAsync(csPath, badCode);

        // Act
        var violations = await _analyzer.CheckHardcodedConnectionStringAsync(_tempWorkspace);

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(violations, v => v.Contains("Phát hiện hardcode Connection String"));
    }
}
