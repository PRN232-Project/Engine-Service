using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PRN232.Domain.Entities;
using PRN232.GradingEngine.Application.DTOs;
using PRN232.GradingEngine.Application.Interfaces;

namespace PRN232.GradingEngine.Application.UseCases.CheckStaticStructure;

public record CheckStaticStructureCommand(
    Guid SubmissionId,
    string WorkspacePath,
    string StudentId,
    Guid ExamId
) : IRequest<GradingReportDto>;

public class CheckStaticStructureCommandHandler : IRequestHandler<CheckStaticStructureCommand, GradingReportDto>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IExamRubricRepository _rubricRepository;
    private readonly IStaticCodeAnalyzer _analyzer;
    private readonly ISolutionBuilder _builder;

    public CheckStaticStructureCommandHandler(
        ISubmissionRepository submissionRepository,
        IExamRubricRepository rubricRepository,
        IStaticCodeAnalyzer analyzer,
        ISolutionBuilder builder)
    {
        _submissionRepository = submissionRepository;
        _rubricRepository = rubricRepository;
        _analyzer = analyzer;
        _builder = builder;
    }

    public async Task<GradingReportDto> Handle(CheckStaticStructureCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy hoặc khởi tạo Submission
        var submission = await _submissionRepository.GetByIdAsync(request.SubmissionId);
        if (submission == null)
        {
            submission = new SubmissionAggregate
            {
                Id = request.SubmissionId,
                StudentId = request.StudentId,
                ExamId = request.ExamId,
                Status = "Running"
            };
            await _submissionRepository.AddAsync(submission);
        }
        else
        {
            // Re-grade: reset trạng thái cũ để tránh giữ lại lỗi cũ từ lần chấm trước.
            submission.Band0Passed = false;
            submission.NamingViolations = new();
            submission.BuildErrors = new();
            submission.ScoreDeductions = 0;
            submission.FinalScore = 0;
            submission.Status = "Running";
            submission.GradedAt = null;
        }

        // 2. Lấy Rubric của kỳ thi
        ExamRubric? rubric = null;
        if (request.ExamId != Guid.Empty)
        {
            rubric = await _rubricRepository.GetByIdAsync(request.ExamId);
        }

        if (rubric == null)
        {
            // Fallback 1: Tìm rubric khớp với tên thư mục workspace (ví dụ: Lab2)
            var allRubrics = await _rubricRepository.GetAllAsync();
            if (allRubrics.Any() && !string.IsNullOrEmpty(request.WorkspacePath))
            {
                var workspaceDirName = Path.GetFileName(request.WorkspacePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                rubric = allRubrics.FirstOrDefault(r => 
                    !string.IsNullOrEmpty(r.ExamCode) && (
                        workspaceDirName.Contains(r.ExamCode, StringComparison.OrdinalIgnoreCase) || 
                        r.ExamCode.Contains(workspaceDirName, StringComparison.OrdinalIgnoreCase)
                    ));

                if (rubric == null)
                {
                    // Fallback 2: Tìm rubric khớp trong toàn bộ đường dẫn
                    rubric = allRubrics.FirstOrDefault(r => 
                        !string.IsNullOrEmpty(r.ExamCode) && 
                        request.WorkspacePath.Contains(r.ExamCode, StringComparison.OrdinalIgnoreCase));
                }
            }

            // Fallback 3: Chọn Rubric đầu tiên trong DB để phục vụ chạy thử nghiệm cục bộ (Local Testing)
            if (rubric == null && allRubrics.Any())
            {
                rubric = allRubrics.First();
            }
        }

        if (rubric == null)
        {
            submission.Status = "Failed";
            submission.BuildErrors.Add($"Không tìm thấy cấu hình Rubric nào trong Database cho ExamId: {request.ExamId} @[{request.WorkspacePath}]");
            await _submissionRepository.SaveChangesAsync();

            return MapToDto(submission);
        }

        // 3. Thực hiện Case 1: Kiểm tra cấu trúc đặt tên (Chỉ trừ điểm, không rớt)
        var namingViolations = await _analyzer.CheckNamingConventionsAsync(
            request.WorkspacePath, 
            request.StudentId, 
            rubric
        );
        submission.RecordNamingResults(namingViolations, rubric.DeductionPointsPerNamingError);

        // 4. Thực hiện Case 2: Kiểm tra Hardcode Connection String (Vi phạm = 0 Điểm luôn)
        var dbViolations = await _analyzer.CheckHardcodedConnectionStringAsync(request.WorkspacePath);
        if (dbViolations.Count > 0)
        {
            submission.RecordBuildFailure(dbViolations.Select(v => $"[DATABASE CONFIG ERROR] {v}").ToList());
            await _submissionRepository.SaveChangesAsync();
            return MapToDto(submission);
        }

        // 5. Thực hiện Case 3: Chạy Dotnet Build tổng thể
        var slnFiles = Directory.GetFiles(request.WorkspacePath, "*.sln", SearchOption.TopDirectoryOnly);
        if (slnFiles.Length == 0)
        {
            submission.RecordBuildFailure(new() { "Không tìm thấy file .sln để thực hiện build." });
            await _submissionRepository.SaveChangesAsync();
            return MapToDto(submission);
        }

        var slnPath = slnFiles[0];
        var (buildSuccess, buildErrors) = await _builder.BuildSolutionAsync(slnPath);

        if (!buildSuccess)
        {
            submission.RecordBuildFailure(buildErrors);
            await _submissionRepository.SaveChangesAsync();
            return MapToDto(submission);
        }

        // 6. Tính điểm cuối cùng nếu vượt qua toàn bộ Band 0
        submission.RecordBand0Success();
        
        var calculatedScore = rubric.MaxScore - submission.ScoreDeductions;
        submission.FinalScore = Math.Max(0, calculatedScore); // Đảm bảo điểm không âm
        submission.GradedAt = DateTime.UtcNow;

        await _submissionRepository.SaveChangesAsync();

        return MapToDto(submission);
    }

    private static GradingReportDto MapToDto(SubmissionAggregate submission)
    {
        return new GradingReportDto
        {
            SubmissionId = submission.Id,
            Band0Passed = submission.Band0Passed,
            NamingViolations = submission.NamingViolations,
            BuildErrors = submission.BuildErrors,
            ScoreDeducted = submission.ScoreDeductions,
            FinalScore = submission.FinalScore,
            Status = submission.Status
        };
    }
}
