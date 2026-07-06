using System;
using System.Collections.Generic;

namespace PRN232.Domain.ValueObjects;

public class TestSectionResult
{
    public string SectionName { get; private set; } = string.Empty;
    public decimal MaxScore { get; private set; }
    public int PassedCount { get; private set; }
    public int TotalCount { get; private set; }
    public decimal Score { get; private set; }
    public string ExecutionLog { get; private set; } = string.Empty;
    public List<string> FailedTests { get; private set; } = new();

    public TestSectionResult() { }

    public TestSectionResult(string sectionName, decimal maxScore, int passedCount, int totalCount, string executionLog, List<string> failedTests)
    {
        SectionName = sectionName;
        MaxScore = maxScore;
        PassedCount = passedCount;
        TotalCount = totalCount;
        ExecutionLog = executionLog ?? string.Empty;
        FailedTests = failedTests ?? new List<string>();

        // Calculate proportional score
        if (TotalCount > 0)
        {
            Score = (decimal)PassedCount / TotalCount * MaxScore;
        }
        else
        {
            Score = 0;
        }
    }
}
