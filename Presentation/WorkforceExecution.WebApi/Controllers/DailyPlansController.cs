using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkforceExecution.Application.Features.DailyPlans.Commands;
using WorkforceExecution.Application.Features.DailyPlans.Queries;
using WorkforceExecution.WebApi.Extensions;

namespace WorkforceExecution.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DailyPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public DailyPlansController(IMediator mediator) => _mediator = mediator;

    // T-1: Teknik Ofis gunluk plan olusturur.
    [HttpPost]
    [Authorize(Roles = "TechOffice")]
    public async Task<IActionResult> Create([FromBody] CreateDailyPlanCommand command)
    {
        command.SubmittedById = User.GetUserId(); // guvenlik: client'tan gelen deger degil, token'daki kimlik
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "TechOffice")]
    public async Task<IActionResult> GetMine([FromQuery] DateTime? date)
    {
        var result = await _mediator.Send(new GetMyPlansQuery { TechOfficeUserId = User.GetUserId(), Date = date });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
