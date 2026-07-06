using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateSubjectRequest
{
    [Required]
    [StringLength(20)]
    public string SubjectCode { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string SubjectName { get; set; } = null!;

    [Range(1, 10)]
    public int Credit { get; set; }
}

public class UpdateSubjectRequest
{
    [Required]
    [StringLength(20)]
    public string SubjectCode { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string SubjectName { get; set; } = null!;

    [Range(1, 10)]
    public int Credit { get; set; }
}

public class PatchSubjectRequest
{
    [StringLength(20)]
    public string? SubjectCode { get; set; }

    [StringLength(200)]
    public string? SubjectName { get; set; }

    [Range(1, 10)]
    public int? Credit { get; set; }
}
