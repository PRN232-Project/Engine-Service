using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Common;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.QueryParams;
using PRN232.LMS.Services.Shared;

namespace PRN232.LMS.API.Controllers;

/// <summary>Manage Students</summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/students")]
[Produces("application/json", "application/xml")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    private readonly IMapper _mapper;

    public StudentsController(IStudentService service, IMapper mapper)
    {
        _service = service;
        _mapper  = mapper;
    }

    /// <summary>Get all students with search, sort, paging, field selection and expand</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StudentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] StudentQueryParams query,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        var result = await _service.GetAllAsync(query);
        var pagedResponse = new PagedResult<StudentResponse>
        {
            Items = result.Items.Select(i => _mapper.Map<StudentResponse>(i)),
            Pagination = result.Pagination
        };
        return Ok(ApiResponse<PagedResult<StudentResponse>>.Ok(pagedResponse));
    }

    /// <summary>Get student by ID</summary>
    /// <param name="id">Student ID</param>
    [HttpGet("{id:int}", Name = "GetStudentById")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm == null) return NotFound(ApiResponse<object>.Fail("Student not found"));
        return Ok(ApiResponse<StudentResponse>.Ok(_mapper.Map<StudentResponse>(bm)));
    }

    /// <summary>Get all enrollments under a specific student</summary>
    [HttpGet("{id:int}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<List<EnrollmentItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollmentsByStudentId([FromRoute] int id)
    {
        var student = await _service.GetByIdAsync(id);
        if (student == null) return NotFound(ApiResponse<object>.Fail("Student not found"));

        var enrollments = await _service.GetEnrollmentsByStudentIdAsync(id);
        var result = enrollments.Select(e => _mapper.Map<EnrollmentItemResponse>(e)).ToList();
        return Ok(ApiResponse<List<EnrollmentItemResponse>>.Ok(result));
    }

    // ── Dưới đây là các endpoint NGOÀI YÊU CẦU LAB1 (LAB chỉ yêu cầu GET) ──────
    // Có thể bỏ comment để dùng khi cần thiết.

    /// <summary>Create a new student</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        try
        {
            var bm       = _mapper.Map<StudentBM>(request);
            var created  = await _service.CreateAsync(bm);
            var response = _mapper.Map<StudentResponse>(created);
            return CreatedAtAction(nameof(GetById), new { id = response.StudentId },
                ApiResponse<StudentResponse>.Ok(response, "Student created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Update an existing student</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        try
        {
            var bm      = _mapper.Map<StudentBM>(request);
            var updated = await _service.UpdateAsync(id, bm);
            if (updated == null) return NotFound(ApiResponse<object>.Fail("Student not found"));
            return Ok(ApiResponse<StudentResponse>.Ok(_mapper.Map<StudentResponse>(updated)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Partially update a student</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] PatchStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));

        try
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound(ApiResponse<object>.Fail("Student not found"));

            if (request.FullName != null) existing.FullName = request.FullName;
            if (request.Email != null) existing.Email = request.Email;
            if (request.DateOfBirth.HasValue) existing.DateOfBirth = request.DateOfBirth.Value;

            var updated = await _service.UpdateAsync(id, existing);
            return Ok(ApiResponse<StudentResponse>.Ok(_mapper.Map<StudentResponse>(updated!)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Delete a student</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Student not found"));
        return NoContent();
    }

    /// <summary>Admin-only endpoint for auditing student count</summary>
    [HttpGet("admin/summary")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminSummary()
    {
        var result = await _service.GetAllAsync(new StudentQueryParams { Page = 1, Size = 1 });
        return Ok(ApiResponse<object>.Ok(new { totalStudents = result.Pagination.TotalItems }, "Admin summary retrieved"));
    }
}
