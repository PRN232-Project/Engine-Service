using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRN232.Domain.Entities;

namespace PRN232.GradingEngine.Infrastructure.Persistence;

public static class DbInitializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task SeedAsync(GradingDbContext context, string rubricSeedFilePath)
    {
        // Tự động chạy migrations nếu DB chưa được cập nhật
        await context.Database.MigrateAsync();

        // 1. Seed/Upsert rubric từ file JSON để tránh hardcode pattern trong code.
        await UpsertRubricsFromFileAsync(context, rubricSeedFilePath);

        // Lấy 1 rubric để gắn dữ liệu mẫu submissions (nếu có)
        var sampleExamId = await context.ExamRubrics.Select(x => x.Id).FirstOrDefaultAsync();
        if (sampleExamId == Guid.Empty)
        {
            return;
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
                    ExamId = sampleExamId,
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
                    ExamId = sampleExamId,
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
                    ExamId = sampleExamId,
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

    private static async Task UpsertRubricsFromFileAsync(GradingDbContext context, string rubricSeedFilePath)
    {
        if (string.IsNullOrWhiteSpace(rubricSeedFilePath) || !File.Exists(rubricSeedFilePath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(rubricSeedFilePath);
        var payload = JsonSerializer.Deserialize<RubricSeedPayload>(json, JsonOptions);
        if (payload?.Rubrics is null || payload.Rubrics.Count == 0)
        {
            return;
        }

        foreach (var item in payload.Rubrics)
        {
            if (string.IsNullOrWhiteSpace(item.ExamCode))
            {
                continue;
            }

            var examCode = item.ExamCode.Trim();
            // Load rubric WITHOUT includes to avoid EF Core relationship tracking/updating conflicts
            var rubric = await context.ExamRubrics
                .FirstOrDefaultAsync(r => r.ExamCode == examCode);

            if (rubric is null)
            {
                rubric = new ExamRubric
                {
                    Id = Guid.NewGuid(),
                    ExamCode = examCode
                };
                await context.ExamRubrics.AddAsync(rubric);
            }
            else
            {
                // Delete old related records directly first and save changes to avoid conflicts
                var oldProjects = await context.RequiredProjects.Where(p => p.ExamRubricId == rubric.Id).ToListAsync();
                if (oldProjects.Any())
                {
                    context.RequiredProjects.RemoveRange(oldProjects);
                }

                var oldFiles = await context.RequiredFiles.Where(f => f.ExamRubricId == rubric.Id).ToListAsync();
                if (oldFiles.Any())
                {
                    context.RequiredFiles.RemoveRange(oldFiles);
                }

                await context.SaveChangesAsync();
            }

            rubric.MaxScore = item.MaxScore;
            rubric.SolutionPattern = item.SolutionPattern;
            rubric.ForbidHardcodedConnectionString = item.ForbidHardcodedConnectionString;
            rubric.DeductionPointsPerNamingError = item.DeductionPointsPerNamingError;

            var newProjects = (item.RequiredProjects ?? new List<ProjectSeedItem>())
                .Where(p => !string.IsNullOrWhiteSpace(p.Pattern))
                .Select(p => new RequiredProject
                {
                    Id = Guid.NewGuid(),
                    ExamRubricId = rubric.Id,
                    Pattern = p.Pattern,
                    MustExist = p.MustExist
                })
                .ToList();

            var newFiles = (item.RequiredFiles ?? new List<FileSeedItem>())
                .Where(f => !string.IsNullOrWhiteSpace(f.Pattern))
                .Select(f => new RequiredFile
                {
                    Id = Guid.NewGuid(),
                    ExamRubricId = rubric.Id,
                    Pattern = f.Pattern,
                    MustExist = f.MustExist
                })
                .ToList();

            await context.RequiredProjects.AddRangeAsync(newProjects);
            await context.RequiredFiles.AddRangeAsync(newFiles);
        }

        await context.SaveChangesAsync();
    }

    private sealed class RubricSeedPayload
    {
        public List<RubricSeedItem> Rubrics { get; set; } = new();
    }

    private sealed class RubricSeedItem
    {
        public string ExamCode { get; set; } = string.Empty;
        public decimal MaxScore { get; set; } = 10.0m;
        public string SolutionPattern { get; set; } = string.Empty;
        public bool ForbidHardcodedConnectionString { get; set; } = true;
        public decimal DeductionPointsPerNamingError { get; set; } = 1.0m;
        public List<ProjectSeedItem> RequiredProjects { get; set; } = new();
        public List<FileSeedItem> RequiredFiles { get; set; } = new();
    }

    private sealed class ProjectSeedItem
    {
        public string Pattern { get; set; } = string.Empty;
        public bool MustExist { get; set; } = true;
    }

    private sealed class FileSeedItem
    {
        public string Pattern { get; set; } = string.Empty;
        public bool MustExist { get; set; } = true;
    }
}
