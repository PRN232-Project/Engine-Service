using System;
using System.Collections.Generic;

namespace PRN232.GradingEngine.Application.DTOs;

public class GradingReportDto
{
    public Guid SubmissionId { get; set; }
    public bool Band0Passed { get; set; }
    public List<string> NamingViolations { get; set; } = new();
    public List<string> BuildErrors { get; set; } = new();
    public decimal ScoreDeducted { get; set; }
    public decimal FinalScore { get; set; }
    public string Status { get; set; } = string.Empty;
}
