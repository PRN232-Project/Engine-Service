using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PRN232.Domain.Entities;
using PRN232.GradingEngine.Application.Interfaces;

namespace PRN232.GradingEngine.Application.UseCases.BuildSolution;

public class BuildSolutionCommand : IRequest<SubmissionAggregate>
{
    public Guid SubmissionId { get; set; }
    public string WorkspacePath { get; set; }

    public BuildSolutionCommand(Guid submissionId, string workspacePath)
    {
        SubmissionId = submissionId;
        WorkspacePath = workspacePath;
    }
}

public class BuildSolutionCommandHandler : IRequestHandler<BuildSolutionCommand, SubmissionAggregate>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly ISolutionBuilder _solutionBuilder;

    public BuildSolutionCommandHandler(ISubmissionRepository submissionRepository, ISolutionBuilder solutionBuilder)
    {
        _submissionRepository = submissionRepository;
        _solutionBuilder = solutionBuilder;
    }

    public async Task<SubmissionAggregate> Handle(BuildSolutionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId);
        if (submission == null)
            throw new Exception($"Submission {request.SubmissionId} not found");

        if (!submission.Band0Passed)
        {
            submission.RecordBand1Result(false, new System.Collections.Generic.List<string> { "Band 0 (Static Structure) did not pass. Cannot build solution." });
            await _submissionRepository.UpdateAsync(submission);
            return submission;
        }

        // Tìm file .sln trong thư mục nộp bài
        var slnFiles = Directory.GetFiles(request.WorkspacePath, "*.sln", SearchOption.AllDirectories);
        if (slnFiles.Length == 0)
        {
            submission.RecordBand1Result(false, new System.Collections.Generic.List<string> { "Không tìm thấy file .sln trong thư mục bài nộp." });
            await _submissionRepository.UpdateAsync(submission);
            return submission;
        }

        // Ưu tiên file sln nằm ở thư mục gốc (hoặc file đầu tiên tìm thấy)
        var slnPath = slnFiles.First();

        var buildResult = await _solutionBuilder.BuildSolutionAsync(slnPath);

        submission.RecordBand1Result(buildResult.Success, buildResult.BuildErrors);

        await _submissionRepository.UpdateAsync(submission);
        await _submissionRepository.SaveChangesAsync();

        return submission;
    }
}
