using System;
using System.Collections.Generic;

namespace PRN232.Domain.Entities;

public class SubmissionAggregate
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    
    public bool Band0Passed { get; set; }
    public bool Band1Passed { get; set; }
    public List<PRN232.Domain.ValueObjects.TestSectionResult> TestSectionResults { get; set; } = new();
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

    public void RecordBand1Result(bool passed, List<string> errors)
    {
        Band1Passed = passed;
        BuildErrors = errors;

        if (!passed)
        {
            Status = "Band1Failed";
            FinalScore = 0;
            GradedAt = DateTime.UtcNow;
        }
        else
        {
            Status = "Band1Passed";
        }
    }

    public void RecordBuildFailure(List<string> errors)
    {
        Band0Passed = false;
        BuildErrors = errors;
        Status = "Failed";
        FinalScore = 0;
        GradedAt = DateTime.UtcNow;
    }

    public void RecordBand0Success()
    {
        Band0Passed = true;
        Status = "Band0Passed";
    }

    public void RecordTestSectionResult(PRN232.Domain.ValueObjects.TestSectionResult result)
    {
        if (!Band1Passed)
            throw new InvalidOperationException("Cannot record test results if Band 1 (Build) has not passed.");

        // Remove old result for this section if exists
        TestSectionResults.RemoveAll(x => x.SectionName == result.SectionName);
        TestSectionResults.Add(result);
    }

    public void CalculateTotalScore()
    {
        if (!Band0Passed || !Band1Passed)
        {
            FinalScore = 0;
            return;
        }

        decimal totalTestScore = 0;
        foreach (var result in TestSectionResults)
        {
            totalTestScore += result.Score;
        }

        FinalScore = totalTestScore - ScoreDeductions;
        if (FinalScore < 0) FinalScore = 0;

        Status = "Graded";
        GradedAt = DateTime.UtcNow;
    }
}
