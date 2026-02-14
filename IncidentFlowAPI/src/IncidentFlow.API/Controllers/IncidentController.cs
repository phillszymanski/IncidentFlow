using IncidentFlow.API.Contracts.Incidents;
using IncidentFlow.Application.Features.Incidents.Commands;
using IncidentFlow.Application.Features.Incidents.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentController : BaseController
    {
        private readonly IMediator _mediator;

        public IncidentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/Incident
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var incidents = await _mediator.Send(new GetAllIncidentsQuery());
            var dto = incidents.Select(i => i.ToResponseDto());
            return HandleResult(dto);
        }

        // GET: api/Incident/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var incident = await _mediator.Send(new GetIncidentByIdQuery(id));
            if (incident is null) return NotFound();

            return HandleResult(incident.ToResponseDto());
        }

        // POST: api/Incident
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IncidentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new CreateIncidentCommand
            {
                Title = dto.Title,
                Description = dto.Description,
                Severity = dto.Severity,
                CreatedBy = dto.CreatedBy,
                AssignedTo = dto.AssignedTo
            };

            var id = await _mediator.Send(command);
            var created = await _mediator.Send(new GetIncidentByIdQuery(id));

            return CreatedAtAction(nameof(Get), new { id }, created?.ToResponseDto());
        }

        // PUT: api/Incident/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] IncidentUpdateDto dto)
        {
            var incident = await _mediator.Send(new UpdateIncidentCommand
            {
                Id = id,
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Severity = dto.Severity,
                AssignedTo = dto.AssignedTo,
                ResolvedAt = dto.ResolvedAt
            });

            if (incident is null) return NotFound();
            return NoContent();
        }

        // DELETE: api/Incident/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteIncidentCommand(id));
            return NoContent();
        }
    }
}