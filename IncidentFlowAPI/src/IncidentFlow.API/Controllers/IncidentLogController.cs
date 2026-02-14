using IncidentFlow.API.Contracts.IncidentLogs;
using IncidentFlow.Application.Features.IncidentLogs.Commands;
using IncidentFlow.Application.Features.IncidentLogs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentLogController : BaseController
{
    private readonly IMediator _mediator;

    public IncidentLogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var logs = await _mediator.Send(new GetAllIncidentLogsQuery());
        var dto = logs.Select(x => x.ToResponseDto());
        return HandleResult(dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var incidentLog = await _mediator.Send(new GetIncidentLogByIdQuery(id));
        if (incidentLog is null) return NotFound();

        return HandleResult(incidentLog.ToResponseDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IncidentLogCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _mediator.Send(new CreateIncidentLogCommand
        {
            IncidentId = dto.IncidentId,
            Action = dto.Action,
            Details = dto.Details,
            PerformedByUserId = dto.PerformedByUserId
        });

        var created = await _mediator.Send(new GetIncidentLogByIdQuery(id));
        return CreatedAtAction(nameof(Get), new { id }, created?.ToResponseDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] IncidentLogUpdateDto dto)
    {
        var incidentLog = await _mediator.Send(new UpdateIncidentLogCommand
        {
            Id = id,
            IncidentId = dto.IncidentId,
            Action = dto.Action,
            Details = dto.Details,
            PerformedByUserId = dto.PerformedByUserId
        });

        if (incidentLog is null) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteIncidentLogCommand(id));
        return NoContent();
    }
}