using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.GradingEngine.Application.DTOs;
using PRN232.GradingEngine.Application.UseCases.CheckStaticStructure;
using PRN232.GradingEngine.Infrastructure.Persistence;

namespace PRN232.GradingEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GradingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly GradingDbContext _context;

    public GradingController(IMediator mediator, GradingDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost("check-band0")]
    public async Task<ActionResult<GradingReportDto>> CheckBand0([FromBody] CheckBand0Request request)
    {
        var command = new CheckStaticStructureCommand(
            request.SubmissionId ?? Guid.NewGuid(),
            request.WorkspacePath,
            request.StudentId,
            request.ExamId
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("samples")]
    public async Task<ActionResult<List<SampleSubmissionDto>>> GetSamples()
    {
        var samples = new List<SampleSubmissionDto>();
        var samplesDir = GetSampleSubmissionsPath();

        if (string.IsNullOrEmpty(samplesDir) || !Directory.Exists(samplesDir))
        {
            return Ok(samples);
        }

        var dirs = Directory.GetDirectories(samplesDir);
        foreach (var dir in dirs)
        {
            var dirName = Path.GetFileName(dir);
            if (Guid.TryParse(dirName, out var submissionId))
            {
                var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId);
                if (submission != null)
                {
                    samples.Add(new SampleSubmissionDto
                    {
                        SubmissionId = submissionId,
                        StudentId = submission.StudentId,
                        ExamId = submission.ExamId,
                        WorkspacePath = dir,
                        DisplayName = $"Học sinh {submission.StudentId} - ID: {submissionId}"
                    });
                }
                else
                {
                    samples.Add(new SampleSubmissionDto
                    {
                        SubmissionId = submissionId,
                        StudentId = "Không rõ",
                        ExamId = Guid.Empty,
                        WorkspacePath = dir,
                        DisplayName = $"Thư mục GUID: {dirName} (Không tìm thấy trong DB)"
                    });
                }
            }
            else
            {
                samples.Add(new SampleSubmissionDto
                {
                    SubmissionId = Guid.Empty,
                    StudentId = "Không rõ",
                    ExamId = Guid.Empty,
                    WorkspacePath = dir,
                    DisplayName = $"Thư mục: {dirName}"
                });
            }
        }

        return Ok(samples);
    }

    private string GetSampleSubmissionsPath()
    {
        var current = Directory.GetCurrentDirectory();
        var path = Path.Combine(current, "sample-student-submission");
        if (Directory.Exists(path)) return Path.GetFullPath(path);

        var parentPath = Path.Combine(current, "..", "sample-student-submission");
        if (Directory.Exists(parentPath)) return Path.GetFullPath(parentPath);

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var baseSub = Path.Combine(baseDir, "sample-student-submission");
        if (Directory.Exists(baseSub)) return Path.GetFullPath(baseSub);

        var parentBaseSub = Path.Combine(baseDir, "..", "..", "..", "sample-student-submission");
        if (Directory.Exists(parentBaseSub)) return Path.GetFullPath(parentBaseSub);

        return string.Empty;
    }
}

public class CheckBand0Request
{
    public Guid? SubmissionId { get; set; }
    public string WorkspacePath { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
}

public class SampleSubmissionDto
{
    public Guid SubmissionId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string WorkspacePath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
