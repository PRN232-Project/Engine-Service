using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRN232.Domain.Entities;

namespace PRN232.GradingEngine.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(GradingDbContext context)
    {
        // Tự động chạy migrations nếu DB chưa được cập nhật
        await context.Database.MigrateAsync();

        // 1. Seed ExamRubrics
        if (!await context.ExamRubrics.AnyAsync())
        {
            var rubrics = new List<ExamRubric>
            {
                new()
                {
                    Id = Guid.Parse("a85590cb-2292-4d7a-8f1d-8cb5d5a712e0"),
                    ExamCode = "PRN231_SU25",
                    MaxScore = 10.0m,
                    SolutionPattern = "^PRN231_SU25_{StudentID}$",
                    ForbidHardcodedConnectionString = true,
                    DeductionPointsPerNamingError = 1.0m,
                    RequiredProjects = new List<RequiredProject>
                    {
                        new() { Id = Guid.NewGuid(), Pattern = "^PRN231_SU25_{StudentID}\\.api$", MustExist = true }
                    },
                    RequiredFiles = new List<RequiredFile>
                    {
                        new() { Id = Guid.NewGuid(), Pattern = "^PRN231_SU25_{StudentID}\\.json$", MustExist = true }
                    }
                },
                new()
                {
                    Id = Guid.Parse("b24590cb-2292-4d7a-8f1d-8cb5d5a712e1"),
                    ExamCode = "PRN230_SU25",
                    MaxScore = 10.0m,
                    SolutionPattern = "^PRN230_SU25_{StudentID}$",
                    ForbidHardcodedConnectionString = true,
                    DeductionPointsPerNamingError = 0.5m,
                    RequiredProjects = new List<RequiredProject>
                    {
                        new() { Id = Guid.NewGuid(), Pattern = "^PRN230_SU25_{StudentID}\\.web$", MustExist = true },
                        new() { Id = Guid.NewGuid(), Pattern = "^PRN230_SU25_{StudentID}\\.service$", MustExist = false }
                    },
                    RequiredFiles = new List<RequiredFile>
                    {
                        new() { Id = Guid.NewGuid(), Pattern = "^appsettings\\.json$", MustExist = true }
                    }
                },
                new()
                {
                    Id = Guid.Parse("c36590cb-2292-4d7a-8f1d-8cb5d5a712e2"),
                    ExamCode = "PRN211_SU25",
                    MaxScore = 10.0m,
                    SolutionPattern = "^PRN211_SU25_{StudentID}$",
                    ForbidHardcodedConnectionString = false,
                    DeductionPointsPerNamingError = 1.5m,
                    RequiredProjects = new List<RequiredProject>
                    {
                        new() { Id = Guid.NewGuid(), Pattern = "^PRN211_SU25_{StudentID}\\.winform$", MustExist = true }
                    },
                    RequiredFiles = new List<RequiredFile>()
                }
            };

            await context.ExamRubrics.AddRangeAsync(rubrics);
            await context.SaveChangesAsync();
        }

        // 2. Seed Submissions
        if (!await context.Submissions.AnyAsync())
        {
            var submissions = new List<SubmissionAggregate>
            {
                new()
                {
                    Id = Guid.Parse("d48590cb-2292-4d7a-8f1d-8cb5d5a712e3"),
                    StudentId = "SE182004",
                    ExamId = Guid.Parse("a85590cb-2292-4d7a-8f1d-8cb5d5a712e0"),
                    Band0Passed = true,
                    NamingViolations = new List<string>(),
                    BuildErrors = new List<string>(),
                    ScoreDeductions = 0.0m,
                    FinalScore = 10.0m,
                    Status = "Band0Passed",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                    GradedAt = DateTime.UtcNow.AddMinutes(-28)
                },
                new()
                {
                    Id = Guid.Parse("e59590cb-2292-4d7a-8f1d-8cb5d5a712e4"),
                    StudentId = "SE171099",
                    ExamId = Guid.Parse("a85590cb-2292-4d7a-8f1d-8cb5d5a712e0"),
                    Band0Passed = false,
                    NamingViolations = new List<string> { "Tên file solution 'WrongName.sln' không đúng cấu trúc bắt buộc." },
                    BuildErrors = new List<string> { "[DATABASE CONFIG ERROR] File DbContext.cs: Phát hiện hardcode Connection String trong hàm gọi 'UseSqlServer'." },
                    ScoreDeductions = 1.0m,
                    FinalScore = 0.0m,
                    Status = "Failed",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-15),
                    GradedAt = DateTime.UtcNow.AddMinutes(-14)
                },
                new()
                {
                    Id = Guid.Parse("f60590cb-2292-4d7a-8f1d-8cb5d5a712e5"),
                    StudentId = "SE160555",
                    ExamId = Guid.Parse("b24590cb-2292-4d7a-8f1d-8cb5d5a712e1"),
                    Band0Passed = false,
                    NamingViolations = new List<string>(),
                    BuildErrors = new List<string> { "Program.cs(12,30): error CS1002: ; expected" },
                    ScoreDeductions = 0.0m,
                    FinalScore = 0.0m,
                    Status = "Failed",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                    GradedAt = DateTime.UtcNow.AddMinutes(-4)
                }
            };

            await context.Submissions.AddRangeAsync(submissions);
            await context.SaveChangesAsync();
        }
    }
}
