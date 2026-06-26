using System;
using System.Collections.Generic;

namespace PRN232.Domain.Entities;

public class SubmissionAggregate
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    
    public bool Band0Passed { get; set; }
    public List<string> NamingViolations { get; set; } = new();
    public List<string> BuildErrors { get; set; } = new();
    public decimal ScoreDeductions { get; set; }
    public decimal FinalScore { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? GradedAt { get; set; }

    public void RecordNamingResults(List<string> violations, decimal pointsPerError)
    {
        NamingViolations = violations;
        ScoreDeductions = violations.Count * pointsPerError;
    }

    public void RecordBuildFailure(List<string> errors)
    {
        BuildErrors = errors;
        Band0Passed = false;
        Status = "Failed";
        FinalScore = 0; // Build fails means 0 points total
        GradedAt = DateTime.UtcNow;
    }

    public void RecordBand0Success()
    {
        Band0Passed = true;
        Status = "Band0Passed";
    }
}
