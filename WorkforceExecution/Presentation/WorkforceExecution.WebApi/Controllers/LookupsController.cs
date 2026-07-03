using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkforceExecution.Application.Features.Lookups.Queries;
using WorkforceExecution.WebApi.Extensions;

namespace WorkforceExecution.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LookupsController(IMediator mediator) => _mediator = mediator;

    // Teknik Ofis sadece kendi bolgesinin lokasyonlarini gorur; PM tumunu gorur.
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _mediator.Send(new GetLookupsQuery
        {
            CrewRegionId = User.IsInRole("ProjectManager") ? null : User.GetCrewRegionId()
        });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
