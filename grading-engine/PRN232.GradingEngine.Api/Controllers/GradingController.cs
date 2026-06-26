using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PRN232.GradingEngine.Application.DTOs;
using PRN232.GradingEngine.Application.UseCases.CheckStaticStructure;

namespace PRN232.GradingEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GradingController : ControllerBase
{
    private readonly IMediator _mediator;

    public GradingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("check-band0")]
    public async Task<ActionResult<GradingReportDto>> CheckBand0([FromBody] CheckBand0Request request)
    {
        var command = new CheckStaticStructureCommand(
            request.SubmissionId ?? Guid.NewGuid(),
            request.WorkspacePath,
            request.StudentId,
            request.ExamId
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public class CheckBand0Request
{
    public Guid? SubmissionId { get; set; }
    public string WorkspacePath { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
}
