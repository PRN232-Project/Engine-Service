using System;
using System.Collections.Generic;

namespace PRN232.Domain.Entities;

public class ExamRubric
{
    public Guid Id { get; set; }
    public string ExamCode { get; set; } = string.Empty;
    public decimal MaxScore { get; set; } = 10.0m;

    // Direct configuration properties (refactored from NamingRuleConfig)
    public string SolutionPattern { get; set; } = string.Empty;
    public bool ForbidHardcodedConnectionString { get; set; } = true;
    public decimal DeductionPointsPerNamingError { get; set; } = 1.0m;

    // Navigation properties for 1-to-many relationships
    public ICollection<RequiredProject> RequiredProjects { get; set; } = new List<RequiredProject>();
    public ICollection<RequiredFile> RequiredFiles { get; set; } = new List<RequiredFile>();
}
