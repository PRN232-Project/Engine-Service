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

/// <summary>Manage Enrollments</summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/enrollments")]
[Produces("application/json", "application/xml")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;
    private readonly IMapper _mapper;

    public EnrollmentsController(IEnrollmentService service, IMapper mapper)
    {
        _service = service;
        _mapper  = mapper;
    }

    /// <summary>Get all enrollments with search, sort, paging, field selection and expand (student, course)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EnrollmentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] EnrollmentQueryParams query)
    {
        var result = await _service.GetAllAsync(query);
        var pagedResponse = new PagedResult<EnrollmentResponse>
        {
            Items = result.Items.Select(i => _mapper.Map<EnrollmentResponse>(i)),
            Pagination = result.Pagination
        };
        return Ok(ApiResponse<PagedResult<EnrollmentResponse>>.Ok(pagedResponse));
    }

    /// <summary>Get enrollment by ID</summary>
    [HttpGet("{id:int}", Name = "GetEnrollmentById")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm == null) return NotFound(ApiResponse<object>.Fail("Enrollment not found"));
        return Ok(ApiResponse<EnrollmentResponse>.Ok(_mapper.Map<EnrollmentResponse>(bm)));
    }

    // ── Dưới đây là các endpoint NGOÀI YÊU CẦU LAB1 (LAB chỉ yêu cầu GET) ──────
    // Có thể bỏ comment để dùng khi cần thiết.

    /// <summary>Create a new enrollment</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        try
        {
            var bm       = _mapper.Map<EnrollmentBM>(request);
            var created  = await _service.CreateAsync(bm);
            var response = _mapper.Map<EnrollmentResponse>(created);
            return CreatedAtAction(nameof(GetById), new { id = response.EnrollmentId },
                ApiResponse<EnrollmentResponse>.Ok(response, "Enrollment created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Update an existing enrollment</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        try
        {
            var bm      = _mapper.Map<EnrollmentBM>(request);
            var updated = await _service.UpdateAsync(id, bm);
            if (updated == null) return NotFound(ApiResponse<object>.Fail("Enrollment not found"));
            return Ok(ApiResponse<EnrollmentResponse>.Ok(_mapper.Map<EnrollmentResponse>(updated)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Partially update an enrollment</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] PatchEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));

        try
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound(ApiResponse<object>.Fail("Enrollment not found"));

            if (request.StudentId.HasValue) existing.StudentId = request.StudentId.Value;
            if (request.CourseId.HasValue) existing.CourseId = request.CourseId.Value;
            if (request.EnrollDate.HasValue) existing.EnrollDate = request.EnrollDate.Value;
            if (request.Status != null) existing.Status = request.Status;

            var updated = await _service.UpdateAsync(id, existing);
            return Ok(ApiResponse<EnrollmentResponse>.Ok(_mapper.Map<EnrollmentResponse>(updated!)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Delete an enrollment</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Enrollment not found"));
        return NoContent();
    }
}
