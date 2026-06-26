using System;

namespace PRN232.Domain.Entities;

public class RequiredProject
{
    public Guid Id { get; set; }
    public Guid ExamRubricId { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public bool MustExist { get; set; } = true;

    // Navigation property
    public ExamRubric ExamRubric { get; set; } = null!;
}
