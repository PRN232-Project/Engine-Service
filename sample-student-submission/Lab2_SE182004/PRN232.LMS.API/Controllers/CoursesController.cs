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

/// <summary>Manage Courses</summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/courses")]
[Produces("application/json", "application/xml")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;
    private readonly IMapper _mapper;

    public CoursesController(ICourseService service, IMapper mapper)
    {
        _service = service;
        _mapper  = mapper;
    }

    /// <summary>Get all courses with search, sort, paging, field selection and expand</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CourseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] CourseQueryParams query)
    {
        var result = await _service.GetAllAsync(query);
        var pagedResponse = new PagedResult<CourseResponse>
        {
            Items = result.Items.Select(i => _mapper.Map<CourseResponse>(i)),
            Pagination = result.Pagination
        };
        return Ok(ApiResponse<PagedResult<CourseResponse>>.Ok(pagedResponse));
    }

    /// <summary>Get course by ID</summary>
    [HttpGet("{id:int}", Name = "GetCourseById")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm == null) return NotFound(ApiResponse<object>.Fail("Course not found"));
        return Ok(ApiResponse<CourseResponse>.Ok(_mapper.Map<CourseResponse>(bm)));
    }

    /// <summary>Get all enrollments under a specific course.</summary>
    [HttpGet("{id:int}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<List<EnrollmentItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollmentsByCourseId([FromRoute] int id, [FromQuery] string? expand)
    {
        var course = await _service.GetByIdAsync(id);
        if (course == null) return NotFound(ApiResponse<object>.Fail("Course not found"));

        var expandParts = expand?.Split(',').Select(p => p.Trim().ToLower()).ToHashSet() ?? new HashSet<string>();
        var enrollments = await _service.GetEnrollmentsByCourseIdAsync(id);
        var result = enrollments.Select(e =>
        {
            var item = _mapper.Map<EnrollmentItemResponse>(e);
            if (!expandParts.Contains("student")) item.Student = null;
            return item;
        }).ToList();

        return Ok(ApiResponse<List<EnrollmentItemResponse>>.Ok(result));
    }

    /// <summary>Get all students enrolled in a specific course.</summary>
    [HttpGet("{courseId:int}/students")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentsByCourseId([FromRoute] int courseId)
    {
        var course = await _service.GetByIdAsync(courseId);
        if (course == null) return NotFound(ApiResponse<object>.Fail("Course not found"));

        var enrollments = await _service.GetEnrollmentsByCourseIdAsync(courseId);
        // Assuming EnrollmentBM has a Student property when properly populated
        var students = enrollments
            .Where(e => e.Student != null)
            .Select(e => _mapper.Map<StudentResponse>(e.Student))
            .ToList();

        return Ok(ApiResponse<List<StudentResponse>>.Ok(students));
    }

    /// <summary>Create a new course</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        try
        {
            var bm       = _mapper.Map<CourseBM>(request);
            var created  = await _service.CreateAsync(bm);
            var response = _mapper.Map<CourseResponse>(created);
            return CreatedAtAction(nameof(GetById), new { id = response.CourseId },
                ApiResponse<CourseResponse>.Ok(response, "Course created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Update an existing course</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        try
        {
            var bm      = _mapper.Map<CourseBM>(request);
            var updated = await _service.UpdateAsync(id, bm);
            if (updated == null) return NotFound(ApiResponse<object>.Fail("Course not found"));
            return Ok(ApiResponse<CourseResponse>.Ok(_mapper.Map<CourseResponse>(updated)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Partially update a course</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] PatchCourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));

        try
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound(ApiResponse<object>.Fail("Course not found"));

            if (request.CourseName != null) existing.CourseName = request.CourseName;
            if (request.SemesterId.HasValue) existing.SemesterId = request.SemesterId.Value;

            var updated = await _service.UpdateAsync(id, existing);
            return Ok(ApiResponse<CourseResponse>.Ok(_mapper.Map<CourseResponse>(updated!)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Delete a course</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Course not found"));
        return NoContent();
    }
}
