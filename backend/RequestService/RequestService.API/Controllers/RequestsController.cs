using Microsoft.AspNetCore.Mvc;
using MediatR;
using RequestService.Application.Commands.CreateLeaveRequest;
using RequestService.Application.Commands.ApproveRequest;
using RequestService.Application.Queries.GetEmployeeRequests;
using RequestService.Domain.Enums;

namespace RequestService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("leave")]
    public async Task<ActionResult<int>> CreateLeaveRequest([FromBody] CreateLeaveRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult> ApproveRequest(int id, [FromHeader(Name = "X-Approver-Role")] string approverRole)
    {
        if (string.IsNullOrEmpty(approverRole))
        {
            return BadRequest("X-Approver-Role header is missing.");
        }

        var command = new ApproveRequestCommand(id, approverRole);
        var result = await _mediator.Send(command);

        if (result) return Ok();
        return BadRequest();
    }

    [HttpPut("{id}/reject")]
    public async Task<ActionResult> RejectRequest(int id)
    {
        var command = new ApproveRequestCommand(id, "Any", false);
        var result = await _mediator.Send(command);

        if (result) return Ok();
        return BadRequest();
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult> GetEmployeeRequests(int employeeId)
    {
        var query = new GetEmployeeRequestsQuery(employeeId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllRequests()
    {
        var query = new GetAllRequestsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRequest(int id)
    {
        var command = new RequestService.Application.Commands.DeleteRequest.DeleteRequestCommand(id);
        var result = await _mediator.Send(command);
        
        if (result) return NoContent();
        return BadRequest();
    }
}
