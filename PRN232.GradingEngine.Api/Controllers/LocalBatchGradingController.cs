using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.Domain.Entities;
using PRN232.Domain.ValueObjects;
using PRN232.GradingEngine.Application.UseCases.BuildSolution;
using PRN232.GradingEngine.Application.UseCases.CheckStaticStructure;
using PRN232.GradingEngine.Application.UseCases.RunTestSection;
using PRN232.GradingEngine.Infrastructure.Persistence;

namespace PRN232.GradingEngine.Api.Controllers;

[ApiController, Route("api/local-grading")]
public class LocalBatchGradingController(IMediator mediator, GradingDbContext db, IHttpClientFactory httpClientFactory, IConfiguration configuration) : ControllerBase
{
    private static readonly JsonSerializerOptions ReportJsonOptions = new(JsonSerializerDefaults.Web);

    [HttpPost("run-batch")]
    public async Task<ActionResult<LocalBatchRunResponse>> RunBatch(LocalBatchRunRequest request, CancellationToken ct)
    {
        if (!Directory.Exists(request.LocalRootPath)) return BadRequest("LocalRootPath không tồn tại trên máy chạy Engine.");
        if (request.ExecutionPackage.Items.Count == 0) return BadRequest("Batch không có grading item.");
        if (!Uri.TryCreate(request.ExecutionPackage.CentralApiBaseUrl, UriKind.Absolute, out var centralUri) || centralUri.Scheme is not ("http" or "https"))
            return BadRequest("CentralApiBaseUrl không hợp lệ.");

        ExamRubric? rubric = null;
        var client = httpClientFactory.CreateClient(nameof(LocalBatchGradingController));
        client.BaseAddress = centralUri;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.ExecutionPackage.ExecutionToken);

        var results = new List<LocalItemRunResult>();
        foreach (var item in request.ExecutionPackage.Items)
        {
            if (ct.IsCancellationRequested) break;
            var match = MatchStudentFolder(request.LocalRootPath, item.StudentCode);
            await PostCallback(client, $"api/integration/grading-items/{item.GradingItemId}/match",
                new { found = match.Path is not null, note = match.Error }, ct);

            if (match.Path is null)
            {
                if (match.IsAmbiguous)
                {
                    await PostTechnicalError(client, item, request.ExecutionPackage.ExamPaper.RubricVersion,
                        "AMBIGUOUS_LOCAL_FOLDER", match.Error ?? "Có nhiều folder khớp mã sinh viên.", ct);
                    results.Add(new(item.GradingItemId, item.StudentCode, "TechnicalError", 0, match.Error ?? ""));
                }
                else
                {
                    results.Add(new(item.GradingItemId, item.StudentCode, "MissingSubmission", null, match.Error ?? ""));
                }
                continue;
            }

            try
            {
                rubric ??= await UpsertRubric(request.ExecutionPackage.ExamPaper, ct);
                var result = await GradeOne(item, match.Path, rubric, request.ExecutionPackage.ExamPaper, ct);
                await RunPlagiarismCheck(client, request.ExecutionPackage, item, match.Path, ct);
                await PostCallback(client, $"api/integration/grading-items/{item.GradingItemId}/attempts", new
                {
                    clientRequestId = Guid.NewGuid().ToString("N"), totalScore = result.TotalScore,
                    rawJsonReport = result.RawJsonReport, hasTechnicalError = false,
                    errorCode = "", errorMessage = "", rubricVersion = request.ExecutionPackage.ExamPaper.RubricVersion,
                    completedAtUtc = DateTime.UtcNow
                }, ct);
                results.Add(new(item.GradingItemId, item.StudentCode, "Graded", result.TotalScore, result.Status));
            }
            catch (Exception ex)
            {
                await PostTechnicalError(client, item, request.ExecutionPackage.ExamPaper.RubricVersion,
                    "ENGINE_EXECUTION_ERROR", ex.Message, ct);
                results.Add(new(item.GradingItemId, item.StudentCode, "TechnicalError", 0, ex.Message));
            }
        }
        return Ok(new LocalBatchRunResponse(request.ExecutionPackage.BatchId, results.Count, results));
    }

    private async Task<GradeOneResult> GradeOne(ExecutionItemContract item, string workspacePath, ExamRubric rubric, ExecutionPaperContract paper, CancellationToken ct)
    {
        var band0 = await mediator.Send(new CheckStaticStructureCommand(item.GradingItemId, workspacePath, item.StudentCode, rubric.Id), ct);
        var submission = await db.Submissions.SingleAsync(x => x.Id == item.GradingItemId, ct);
        if (!submission.Band0Passed)
            return BuildResult(submission, paper, "Band0Failed");

        submission = await mediator.Send(new BuildSolutionCommand(item.GradingItemId, workspacePath), ct);
        if (!submission.Band1Passed)
            return BuildResult(submission, paper, "Band1Failed");

        foreach (var section in paper.Sections)
        {
            submission = await mediator.Send(new RunTestSectionCommand(
                item.GradingItemId, workspacePath, section.Name, section.Weight,
                section.TestCases?.ToList() ?? [], section.ApiProjectPath), ct);
        }
        if (paper.Sections.Count > 0) submission.CalculateTotalScore();
        else submission.FinalScore = band0.FinalScore;
        await db.SaveChangesAsync(ct);
        return BuildResult(submission, paper, submission.Status);
    }

    private static GradeOneResult BuildResult(SubmissionAggregate submission, ExecutionPaperContract paper, string status)
    {
        var report = new
        {
            submissionId = submission.Id, studentCode = submission.StudentId, examPaperCode = paper.Code,
            rubricVersion = paper.RubricVersion, band0Passed = submission.Band0Passed,
            band1Passed = submission.Band1Passed, namingViolations = submission.NamingViolations,
            buildErrors = submission.BuildErrors, scoreDeducted = submission.ScoreDeductions,
            sectionResults = submission.TestSectionResults.Select(x => new
            {
                name = x.SectionName, score = x.Score, maxScore = x.MaxScore,
                status = x.TotalCount > 0 && x.PassedCount == x.TotalCount ? "Passed" : "Failed",
                feedback = x.ExecutionLog, failedTests = x.FailedTests
            }), totalScore = submission.FinalScore, status
        };
        return new GradeOneResult(submission.FinalScore, status, JsonSerializer.Serialize(report, ReportJsonOptions));
    }

    private async Task<ExamRubric> UpsertRubric(ExecutionPaperContract paper, CancellationToken ct)
    {
        var rubric = await db.ExamRubrics.Include(x => x.RequiredProjects).Include(x => x.RequiredFiles)
            .SingleOrDefaultAsync(x => x.ExamCode == paper.Code, ct);
        if (rubric is null) { rubric = new ExamRubric { Id = Guid.NewGuid(), ExamCode = paper.Code }; db.ExamRubrics.Add(rubric); }
        else { db.RequiredProjects.RemoveRange(rubric.RequiredProjects); db.RequiredFiles.RemoveRange(rubric.RequiredFiles); }
        rubric.MaxScore = paper.MaxScore; rubric.SolutionPattern = NormalizeSolutionPattern(paper.SolutionPattern);
        rubric.ForbidHardcodedConnectionString = paper.ForbidHardcodedConnectionString;
        rubric.DeductionPointsPerNamingError = 1m;
        if (paper.RequireAppSettings)
            db.RequiredFiles.Add(new RequiredFile { Id = Guid.NewGuid(), ExamRubricId = rubric.Id, Pattern = "^appsettings\\.json$", MustExist = true });
        await db.SaveChangesAsync(ct); return rubric;
    }

    private static string NormalizeSolutionPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern) || pattern == "*.sln") return ".*";
        var withoutExtension = pattern.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ? Path.GetFileNameWithoutExtension(pattern) : pattern;
        if (withoutExtension.Contains('*') && !withoutExtension.StartsWith('^')) return "^" + Regex.Escape(withoutExtension).Replace("\\*", ".*") + "$";
        return withoutExtension;
    }

    private static FolderMatch MatchStudentFolder(string root, string studentCode)
    {
        var directories = Directory.GetDirectories(root, "*", SearchOption.TopDirectoryOnly);
        var exact = directories.Where(x => string.Equals(Path.GetFileName(x), studentCode, StringComparison.OrdinalIgnoreCase)).ToList();
        if (exact.Count == 1) return new(exact[0], null, false);
        var candidates = directories.Where(x => Path.GetFileName(x).Contains(studentCode, StringComparison.OrdinalIgnoreCase)).ToList();
        return candidates.Count switch
        {
            1 => new(candidates[0], null, false),
            > 1 => new(null, $"Có {candidates.Count} folder khớp mã {studentCode}.", true),
            _ => new(null, $"Không tìm thấy folder cho {studentCode}.", false)
        };
    }

    private static async Task PostCallback(HttpClient client, string path, object body, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync(path, body, ct);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Central callback {path} thất bại: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync(ct)}");
    }

    private static Task PostTechnicalError(HttpClient client, ExecutionItemContract item, string rubricVersion, string code, string message, CancellationToken ct) =>
        PostCallback(client, $"api/integration/grading-items/{item.GradingItemId}/attempts", new
        {
            clientRequestId = Guid.NewGuid().ToString("N"), totalScore = 0, rawJsonReport = "",
            hasTechnicalError = true, errorCode = code, errorMessage = message,
            rubricVersion, completedAtUtc = DateTime.UtcNow
        }, ct);

    private async Task RunPlagiarismCheck(HttpClient centralClient, BatchExecutionPackageContract package,
        ExecutionItemContract item, string workspacePath, CancellationToken ct)
    {
        try
        {
            var plagiarismBaseUrl = configuration["PlagiarismService:BaseUrl"] ?? "http://localhost:5175";
            var plagiarismClient = httpClientFactory.CreateClient("PlagiarismService");
            plagiarismClient.BaseAddress = new Uri(plagiarismBaseUrl.TrimEnd('/') + "/");

            using var checkResponse = await plagiarismClient.PostAsJsonAsync("api/Plagiarism/check", new
            {
                submissionId = item.GradingItemId,
                examId = package.ExamSessionId,
                studentId = item.StudentCode,
                workspacePath,
                bannedKeywords = package.ExamPaper.PlagiarismKeywords
            }, ct);
            checkResponse.EnsureSuccessStatusCode();

            var report = await plagiarismClient.GetFromJsonAsync<PlagiarismReportContract>(
                $"api/Plagiarism/submissions/{item.GradingItemId}", ReportJsonOptions, ct)
                ?? throw new InvalidOperationException("Plagiarism Service không trả về báo cáo.");
            var comparisons = await plagiarismClient.GetFromJsonAsync<List<PlagiarismComparisonContract>>(
                $"api/Plagiarism/exams/{package.ExamSessionId}/comparisons", ReportJsonOptions, ct) ?? [];
            var related = comparisons.Where(x => x.SubmissionIdA == item.GradingItemId || x.SubmissionIdB == item.GradingItemId).ToList();
            var maxSimilarity = related.Count == 0 ? 0m : related.Max(x => x.SimilarityScore);
            var rawReport = JsonSerializer.Serialize(new { report, comparisons = related }, ReportJsonOptions);

            await PostCallback(centralClient, $"api/integration/grading-items/{item.GradingItemId}/plagiarism", new
            {
                status = "Completed",
                violationCount = report.Violations.Count,
                maxSimilarity,
                rawJsonReport = rawReport,
                errorMessage = "",
                checkedAtUtc = report.ScannedAt
            }, ct);
        }
        catch (Exception ex)
        {
            await PostCallback(centralClient, $"api/integration/grading-items/{item.GradingItemId}/plagiarism", new
            {
                status = "TechnicalError",
                violationCount = 0,
                maxSimilarity = 0,
                rawJsonReport = "{}",
                errorMessage = ex.Message,
                checkedAtUtc = DateTime.UtcNow
            }, ct);
        }
    }

    private record FolderMatch(string? Path, string? Error, bool IsAmbiguous);
    private record GradeOneResult(decimal TotalScore, string Status, string RawJsonReport);
}

public record LocalBatchRunRequest(string LocalRootPath, BatchExecutionPackageContract ExecutionPackage);
public record BatchExecutionPackageContract(Guid BatchId, string BatchCode, Guid ExamSessionId, string ExamSessionCode, ExecutionPaperContract ExamPaper, IReadOnlyList<ExecutionItemContract> Items, string ExecutionToken, DateTime ExpiresAtUtc, string CentralApiBaseUrl);
public record ExecutionPaperContract(Guid Id, string Code, string RubricVersion, decimal MaxScore, string SolutionPattern, bool RequireAppSettings, bool ForbidHardcodedConnectionString, int TimeoutSeconds, IReadOnlyList<string> PlagiarismKeywords, IReadOnlyList<ExecutionSectionContract> Sections);
public record ExecutionSectionContract(string Name, decimal Weight, string TestFilter, IReadOnlyList<ApiTestCase>? TestCases, string? ApiProjectPath);
public record ExecutionItemContract(Guid GradingItemId, string StudentCode, string StudentName, string PaperCode);
public record LocalItemRunResult(Guid GradingItemId, string StudentCode, string Status, decimal? TotalScore, string Message);
public record LocalBatchRunResponse(Guid BatchId, int ProcessedCount, IReadOnlyList<LocalItemRunResult> Items);
public record PlagiarismReportContract(Guid Id, Guid SubmissionId, Guid ExamId, string StudentId, bool HasViolations, DateTime ScannedAt, IReadOnlyList<PlagiarismViolationContract> Violations);
public record PlagiarismViolationContract(string FileName, string BannedKeyword, int LineNumber, string CodeSnippet);
public record PlagiarismComparisonContract(Guid Id, Guid ExamId, Guid SubmissionIdA, Guid SubmissionIdB, string StudentIdA, string StudentIdB, decimal SimilarityScore, bool GuidMatched, DateTime ScannedAt);
