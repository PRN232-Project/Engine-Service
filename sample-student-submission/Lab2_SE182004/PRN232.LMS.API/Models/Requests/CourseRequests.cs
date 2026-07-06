using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateCourseRequest
{
    [Required]
    [StringLength(200)]
    public string CourseName { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int SemesterId { get; set; }
}

public class UpdateCourseRequest
{
    [Required]
    [StringLength(200)]
    public string CourseName { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int SemesterId { get; set; }
}

public class PatchCourseRequest
{
    [StringLength(200)]
    public string? CourseName { get; set; }

    [Range(1, int.MaxValue)]
    public int? SemesterId { get; set; }
}
