using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PRN232.Domain.Entities;
using PRN232.Domain.ValueObjects;
using PRN232.GradingEngine.Application.Interfaces;

namespace PRN232.GradingEngine.Application.UseCases.RunTestSection;

public class RunTestSectionCommand : IRequest<SubmissionAggregate>
{
    public Guid SubmissionId { get; set; }
    public string WorkspacePath { get; set; }
    public string SectionName { get; set; }
    public decimal MaxScore { get; set; }
    public List<ApiTestCase> TestCases { get; set; }
    public string? ApiProjectPath { get; set; }

    public RunTestSectionCommand(Guid submissionId, string workspacePath, string sectionName, decimal maxScore, List<ApiTestCase> testCases, string? apiProjectPath = null)
    {
        SubmissionId = submissionId;
        WorkspacePath = workspacePath;
        SectionName = sectionName;
        MaxScore = maxScore;
        TestCases = testCases ?? new List<ApiTestCase>();
        ApiProjectPath = apiProjectPath;
    }
}

public class RunTestSectionCommandHandler : IRequestHandler<RunTestSectionCommand, SubmissionAggregate>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly ITestRunner _testRunner;

    public RunTestSectionCommandHandler(ISubmissionRepository submissionRepository, ITestRunner testRunner)
    {
        _submissionRepository = submissionRepository;
        _testRunner = testRunner;
    }

    public async Task<SubmissionAggregate> Handle(RunTestSectionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId);
        if (submission == null)
            throw new Exception($"Submission {request.SubmissionId} not found");

        if (!submission.Band1Passed)
        {
            throw new InvalidOperationException("Cannot run test section if Band 1 has not passed.");
        }

        // Tìm folder chứa dự án của sinh viên (chứa .sln hoặc .csproj)
        var projectDir = request.WorkspacePath;

        // Chạy test runner động
        var testResult = await _testRunner.RunApiTestsAsync(projectDir, request.SectionName, request.MaxScore, request.TestCases, request.ApiProjectPath);

        // Lưu kết quả vào Aggregate
        submission.RecordTestSectionResult(testResult);
        
        await _submissionRepository.UpdateAsync(submission);
        await _submissionRepository.SaveChangesAsync();

        return submission;
    }
}
