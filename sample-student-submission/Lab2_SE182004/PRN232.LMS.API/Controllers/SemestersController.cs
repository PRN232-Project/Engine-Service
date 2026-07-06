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

/// <summary>Manage Semesters</summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/semesters")]
[Produces("application/json", "application/xml")]
[Authorize]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _service;
    private readonly IMapper _mapper;

    public SemestersController(ISemesterService service, IMapper mapper)
    {
        _service = service;
        _mapper  = mapper;
    }

    /// <summary>Get all semesters with search, sort and paging</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SemesterResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SemesterQueryParams query)
    {
        var result = await _service.GetAllAsync(query);
        var pagedResponse = new PagedResult<SemesterResponse>
        {
            Items = result.Items.Select(i => _mapper.Map<SemesterResponse>(i)),
            Pagination = result.Pagination
        };
        return Ok(ApiResponse<PagedResult<SemesterResponse>>.Ok(pagedResponse));
    }

    /// <summary>Get semester by ID</summary>
    [HttpGet("{id:int}", Name = "GetSemesterById")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm == null) return NotFound(ApiResponse<object>.Fail("Semester not found"));
        return Ok(ApiResponse<SemesterResponse>.Ok(_mapper.Map<SemesterResponse>(bm)));
    }

    /// <summary>Get all courses under a specific semester</summary>
    [HttpGet("{id:int}/courses")]
    [ProducesResponseType(typeof(ApiResponse<List<CourseResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoursesBySemesterId([FromRoute] int id)
    {
        var semester = await _service.GetByIdAsync(id);
        if (semester == null) return NotFound(ApiResponse<object>.Fail("Semester not found"));

        var courses = await _service.GetCoursesBySemesterIdAsync(id);
        var result = courses.Select(c => _mapper.Map<CourseResponse>(_mapper.Map<CourseBM>(c))).ToList();
        return Ok(ApiResponse<List<CourseResponse>>.Ok(result));
    }

    // ── Dưới đây là các endpoint NGOÀI YÊU CẦU LAB1 (LAB chỉ yêu cầu GET) ──────
    // Có thể bỏ comment để dùng khi cần thiết.

    /// <summary>Create a new semester</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        var bm       = _mapper.Map<SemesterBM>(request);
        var created  = await _service.CreateAsync(bm);
        var response = _mapper.Map<SemesterResponse>(created);
        return CreatedAtAction(nameof(GetById), new { id = response.SemesterId },
            ApiResponse<SemesterResponse>.Ok(response, "Semester created successfully"));
    }

    /// <summary>Update an existing semester</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSemesterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        var bm      = _mapper.Map<SemesterBM>(request);
        var updated = await _service.UpdateAsync(id, bm);
        if (updated == null) return NotFound(ApiResponse<object>.Fail("Semester not found"));
        return Ok(ApiResponse<SemesterResponse>.Ok(_mapper.Map<SemesterResponse>(updated)));
    }

    /// <summary>Delete a semester</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Semester not found"));
        return Ok(ApiResponse<object>.Ok(null!, "Semester deleted successfully"));
    }
}
