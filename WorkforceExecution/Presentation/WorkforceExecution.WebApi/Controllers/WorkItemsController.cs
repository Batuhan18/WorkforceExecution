using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkforceExecution.Application.Features.WorkItems.Commands;
using WorkforceExecution.Application.Features.WorkItems.Queries;
using WorkforceExecution.WebApi.Extensions;

namespace WorkforceExecution.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkItemsController(IMediator mediator) => _mediator = mediator;

    // T0: HoM kendisine atanan isleri goruntuler.
    [HttpGet("assigned")]
    [Authorize(Roles = "HeadOfMaster")]
    public async Task<IActionResult> GetAssigned([FromQuery] DateTime? date)
    {
        var result = await _mediator.Send(new GetAssignedWorkItemsQuery { HomUserId = User.GetUserId(), Date = date });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // T0: HoM crew olusturur ve isi alir.
    [HttpPost("{id:int}/crew")]
    [Authorize(Roles = "HeadOfMaster")]
    public async Task<IActionResult> AssignCrew(int id, [FromBody] AssignCrewCommand command)
    {
        command.WorkItemId = id;
        command.HomUserId = User.GetUserId();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Gun sonu: gerceklesen degerler girilir, kayit onaya dusar.
    [HttpPost("{id:int}/fact")]
    [Authorize(Roles = "HeadOfMaster")]
    public async Task<IActionResult> SubmitFact(int id, [FromBody] SubmitFactCommand command)
    {
        command.WorkItemId = id;
        command.HomUserId = User.GetUserId();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Onay zinciri: HoM -> Site Chief -> PM (yetki kontrolu handler'da).
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "HeadOfMaster,SiteChief,ProjectManager")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveWorkItemCommand command)
    {
        command.WorkItemId = id;
        command.ApproverId = User.GetUserId();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "HeadOfMaster,SiteChief,ProjectManager")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectWorkItemCommand command)
    {
        command.WorkItemId = id;
        command.ApproverId = User.GetUserId();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Rolun onay kuyrugu.
    [HttpGet("pending-approvals")]
    [Authorize(Roles = "HeadOfMaster,SiteChief,ProjectManager")]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] DateTime? date)
    {
        var result = await _mediator.Send(new GetPendingApprovalsQuery { UserId = User.GetUserId(), Date = date });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
