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
using PRN232.GradingEngine.Application.UseCases.BuildSolution;
using PRN232.GradingEngine.Application.UseCases.RunTestSection;
using PRN232.Domain.ValueObjects;
using PRN232.Domain.Entities;
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
        var subId = request.SubmissionId;
        if (subId == null || subId == Guid.Empty)
        {
            subId = Guid.NewGuid();
        }

        var command = new CheckStaticStructureCommand(
            subId.Value,
            request.WorkspacePath,
            request.StudentId,
            request.ExamId
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("check-band1")]
    public async Task<IActionResult> CheckBand1([FromBody] CheckBand1Request request)
    {
        var command = new BuildSolutionCommand(request.SubmissionId, request.WorkspacePath);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("check-band2")]
    public async Task<IActionResult> CheckBand2([FromBody] CheckBand2Request request)
    {
        var command = new RunTestSectionCommand(
            request.SubmissionId,
            request.WorkspacePath,
            request.SectionName,
            request.MaxScore,
            request.TestCases,
            request.ApiProjectPath
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

    [HttpGet("projects")]
    public IActionResult GetProjects([FromQuery] string workspacePath)
    {
        if (string.IsNullOrWhiteSpace(workspacePath) || !Directory.Exists(workspacePath))
        {
            return Ok(new List<string>());
        }

        try
        {
            var csprojFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);
            var relativePaths = csprojFiles.Select(f => Path.GetRelativePath(workspacePath, f).Replace('\\', '/')).ToList();
            return Ok(relativePaths);
        }
        catch (Exception ex)
        {
            return BadRequest($"Lỗi khi quét project: {ex.Message}");
        }
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

    private RubricResponseDto MapToDto(ExamRubric rubric)
    {
        return new RubricResponseDto
        {
            Id = rubric.Id,
            ExamCode = rubric.ExamCode,
            MaxScore = rubric.MaxScore,
            SolutionPattern = rubric.SolutionPattern,
            ForbidHardcodedConnectionString = rubric.ForbidHardcodedConnectionString,
            DeductionPointsPerNamingError = rubric.DeductionPointsPerNamingError,
            RequiredProjects = rubric.RequiredProjects.Select(p => new RequiredItemDto
            {
                Pattern = p.Pattern,
                MustExist = p.MustExist
            }).ToList(),
            RequiredFiles = rubric.RequiredFiles.Select(f => new RequiredItemDto
            {
                Pattern = f.Pattern,
                MustExist = f.MustExist
            }).ToList()
        };
    }

    [HttpGet("rubrics")]
    public async Task<ActionResult<List<RubricResponseDto>>> GetRubrics()
    {
        var rubrics = await _context.ExamRubrics
            .Include(r => r.RequiredProjects)
            .Include(r => r.RequiredFiles)
            .ToListAsync();
        return Ok(rubrics.Select(MapToDto).ToList());
    }

    [HttpGet("rubrics/{examCode}")]
    public async Task<ActionResult<RubricResponseDto>> GetRubric(string examCode)
    {
        var rubric = await _context.ExamRubrics
            .Include(r => r.RequiredProjects)
            .Include(r => r.RequiredFiles)
            .FirstOrDefaultAsync(r => r.ExamCode.ToLower() == examCode.ToLower().Trim());

        if (rubric == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(rubric));
    }

    [HttpPost("rubrics")]
    public async Task<IActionResult> UpsertRubric([FromBody] RubricDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ExamCode))
        {
            return BadRequest("ExamCode is required");
        }

        var examCode = dto.ExamCode.Trim();
        var rubric = await _context.ExamRubrics
            .FirstOrDefaultAsync(r => r.ExamCode.ToLower() == examCode.ToLower());

        if (rubric == null)
        {
            rubric = new ExamRubric
            {
                Id = Guid.NewGuid(),
                ExamCode = examCode
            };
            await _context.ExamRubrics.AddAsync(rubric);
        }
        else
        {
            // Delete old items to avoid tracking issues
            var oldProjects = await _context.RequiredProjects.Where(p => p.ExamRubricId == rubric.Id).ToListAsync();
            if (oldProjects.Any())
            {
                _context.RequiredProjects.RemoveRange(oldProjects);
            }

            var oldFiles = await _context.RequiredFiles.Where(f => f.ExamRubricId == rubric.Id).ToListAsync();
            if (oldFiles.Any())
            {
                _context.RequiredFiles.RemoveRange(oldFiles);
            }

            await _context.SaveChangesAsync();
        }

        rubric.MaxScore = dto.MaxScore;
        rubric.SolutionPattern = dto.SolutionPattern;
        rubric.ForbidHardcodedConnectionString = dto.ForbidHardcodedConnectionString;
        rubric.DeductionPointsPerNamingError = dto.DeductionPointsPerNamingError;

        var newProjects = dto.RequiredProjects
            .Where(p => !string.IsNullOrWhiteSpace(p.Pattern))
            .Select(p => new RequiredProject
            {
                Id = Guid.NewGuid(),
                ExamRubricId = rubric.Id,
                Pattern = p.Pattern,
                MustExist = p.MustExist
            })
            .ToList();

        var newFiles = dto.RequiredFiles
            .Where(f => !string.IsNullOrWhiteSpace(f.Pattern))
            .Select(f => new RequiredFile
            {
                Id = Guid.NewGuid(),
                ExamRubricId = rubric.Id,
                Pattern = f.Pattern,
                MustExist = f.MustExist
            })
            .ToList();

        await _context.RequiredProjects.AddRangeAsync(newProjects);
        await _context.RequiredFiles.AddRangeAsync(newFiles);

        await _context.SaveChangesAsync();

        var savedRubric = await _context.ExamRubrics
            .Include(r => r.RequiredProjects)
            .Include(r => r.RequiredFiles)
            .FirstOrDefaultAsync(r => r.Id == rubric.Id);

        return Ok(MapToDto(savedRubric!));
    }
}

public class RubricResponseDto
{
    public Guid Id { get; set; }
    public string ExamCode { get; set; } = string.Empty;
    public decimal MaxScore { get; set; } = 10.0m;
    public string SolutionPattern { get; set; } = string.Empty;
    public bool ForbidHardcodedConnectionString { get; set; } = true;
    public decimal DeductionPointsPerNamingError { get; set; } = 1.0m;
    public List<RequiredItemDto> RequiredProjects { get; set; } = new();
    public List<RequiredItemDto> RequiredFiles { get; set; } = new();
}

public class RubricDto
{
    public string ExamCode { get; set; } = string.Empty;
    public decimal MaxScore { get; set; } = 10.0m;
    public string SolutionPattern { get; set; } = string.Empty;
    public bool ForbidHardcodedConnectionString { get; set; } = true;
    public decimal DeductionPointsPerNamingError { get; set; } = 1.0m;
    public List<RequiredItemDto> RequiredProjects { get; set; } = new();
    public List<RequiredItemDto> RequiredFiles { get; set; } = new();
}

public class RequiredItemDto
{
    public string Pattern { get; set; } = string.Empty;
    public bool MustExist { get; set; } = true;
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

public class CheckBand1Request
{
    public Guid SubmissionId { get; set; }
    public string WorkspacePath { get; set; } = string.Empty;
}

public class CheckBand2Request
{
    public Guid SubmissionId { get; set; }
    public string WorkspacePath { get; set; } = string.Empty;
    public string SectionName { get; set; } = "Band2";
    public decimal MaxScore { get; set; } = 30;
    public List<ApiTestCase> TestCases { get; set; } = new();
    public string? ApiProjectPath { get; set; }
}
