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

/// <summary>Manage Subjects</summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/subjects")]
[Produces("application/json", "application/xml")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _service;
    private readonly IMapper _mapper;

    public SubjectsController(ISubjectService service, IMapper mapper)
    {
        _service = service;
        _mapper  = mapper;
    }

    /// <summary>Get all subjects with search, sort and paging</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SubjectResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SubjectQueryParams query)
    {
        var result = await _service.GetAllAsync(query);
        var pagedResponse = new PagedResult<SubjectResponse>
        {
            Items = result.Items.Select(i => _mapper.Map<SubjectResponse>(i)),
            Pagination = result.Pagination
        };
        return Ok(ApiResponse<PagedResult<SubjectResponse>>.Ok(pagedResponse));
    }

    /// <summary>Get subject by ID</summary>
    [HttpGet("{id:int}", Name = "GetSubjectById")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var bm = await _service.GetByIdAsync(id);
        if (bm == null) return NotFound(ApiResponse<object>.Fail("Subject not found"));
        return Ok(ApiResponse<SubjectResponse>.Ok(_mapper.Map<SubjectResponse>(bm)));
    }

    // ── Dưới đây là các endpoint NGOÀI YÊU CẦU LAB1 (LAB chỉ yêu cầu GET) ──────
    // Có thể bỏ comment để dùng khi cần thiết.

    /// <summary>Create a new subject</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        try
        {
            var bm       = _mapper.Map<SubjectBM>(request);
            var created  = await _service.CreateAsync(bm);
            var response = _mapper.Map<SubjectResponse>(created);
            return CreatedAtAction(nameof(GetById), new { id = response.SubjectId },
                ApiResponse<SubjectResponse>.Ok(response, "Subject created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Update an existing subject</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));
    
        try
        {
            var bm      = _mapper.Map<SubjectBM>(request);
            var updated = await _service.UpdateAsync(id, bm);
            if (updated == null) return NotFound(ApiResponse<object>.Fail("Subject not found"));
            return Ok(ApiResponse<SubjectResponse>.Ok(_mapper.Map<SubjectResponse>(updated)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Partially update a subject</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] PatchSubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request", ModelState));

        try
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound(ApiResponse<object>.Fail("Subject not found"));

            if (request.SubjectCode != null) existing.SubjectCode = request.SubjectCode;
            if (request.SubjectName != null) existing.SubjectName = request.SubjectName;
            if (request.Credit.HasValue) existing.Credit = request.Credit.Value;

            var updated = await _service.UpdateAsync(id, existing);
            return Ok(ApiResponse<SubjectResponse>.Ok(_mapper.Map<SubjectResponse>(updated!)));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }

    /// <summary>Delete a subject</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Subject not found"));
        return NoContent();
    }
}
