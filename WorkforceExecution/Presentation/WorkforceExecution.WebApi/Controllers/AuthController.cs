using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkforceExecution.Application.Features.Auth.Commands;

namespace WorkforceExecution.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : Unauthorized(result);
    }
}
