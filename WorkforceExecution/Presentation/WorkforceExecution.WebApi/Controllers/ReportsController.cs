using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkforceExecution.Application.Features.Reports.Queries;

namespace WorkforceExecution.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    // Onaylanan kayitlarin konsolide edildigi Daily Report + KPI'lar.
    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily([FromQuery] DateTime? date)
    {
        var result = await _mediator.Send(new GetDailyReportQuery { Date = date ?? DateTime.UtcNow.Date });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
