using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateEnrollmentRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    [Required]
    public DateTime EnrollDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = null!;
}

public class UpdateEnrollmentRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    [Required]
    public DateTime EnrollDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = null!;
}

public class PatchEnrollmentRequest
{
    [Range(1, int.MaxValue)]
    public int? StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int? CourseId { get; set; }

    public DateTime? EnrollDate { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }
}
