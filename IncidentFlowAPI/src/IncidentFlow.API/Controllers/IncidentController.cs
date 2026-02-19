using IncidentFlow.API.Contracts.Incidents;
using IncidentFlow.API.Authorization;
using IncidentFlow.Application.Features.Incidents.Commands;
using IncidentFlow.Application.Features.Incidents.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Policy = PolicyConstants.CanReadIncidents)]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string filter = "total")
        {
            var normalizedPage = Math.Max(1, page);
            var normalizedPageSize = Math.Clamp(pageSize, 1, 50);
            var userIdClaim = User.FindFirst("userId")?.Value;
            var currentUserId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : (Guid?)null;

            var parsedFilter = Enum.TryParse<IncidentListFilterType>(filter, true, out var filterType)
                ? filterType
                : IncidentListFilterType.Total;

            var result = await _mediator.Send(new GetAllIncidentsQuery(
                normalizedPage,
                normalizedPageSize,
                parsedFilter,
                currentUserId));
            var dto = new PagedIncidentResponseDto
            {
                Items = result.Items.Select(i => i.ToResponseDto()).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return HandleResult(dto);
        }

        [Authorize(Policy = PolicyConstants.CanReadIncidents)]
        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            var currentUserId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : (Guid?)null;
            var summary = await _mediator.Send(new GetIncidentDashboardSummaryQuery(currentUserId));
            return HandleResult(summary.ToResponseDto());
        }

        // GET: api/Incident/{id}
        [Authorize(Policy = PolicyConstants.CanReadIncidents)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var incident = await _mediator.Send(new GetIncidentByIdQuery(id));
            if (incident is null) return NotFound();

            return HandleResult(incident.ToResponseDto());
        }

        // POST: api/Incident
        [Authorize(Policy = PolicyConstants.CanCreateIncidents)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IncidentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.AssignedTo.HasValue && !User.HasClaim("permission", PermissionConstants.IncidentsAssign))
            {
                return Forbid();
            }

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var createdByUserId))
            {
                return Unauthorized("Missing authenticated user context.");
            }

            var command = new CreateIncidentCommand
            {
                Title = dto.Title,
                Description = dto.Description,
                Severity = dto.Severity,
                CreatedBy = createdByUserId,
                AssignedTo = dto.AssignedTo
            };

            var id = await _mediator.Send(command);
            var created = await _mediator.Send(new GetIncidentByIdQuery(id));

            return CreatedAtAction(nameof(Get), new { id }, created?.ToResponseDto());
        }

        // PUT: api/Incident/{id}
        [Authorize(Policy = PolicyConstants.CanReadIncidents)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] IncidentUpdateDto dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var performedByUserId))
            {
                return Unauthorized("Missing authenticated user context.");
            }

            var currentIncident = await _mediator.Send(new GetIncidentByIdQuery(id));
            if (currentIncident is null)
            {
                return NotFound();
            }

            var canEditAny = User.HasClaim("permission", PermissionConstants.IncidentsEditAny);
            var canEditOwn = User.HasClaim("permission", PermissionConstants.IncidentsEditOwn);
            var isOwnedIncident = currentIncident.CreatedBy == performedByUserId || currentIncident.AssignedTo == performedByUserId;

            var isUpdatingStatusOnly = dto.Status.HasValue
                && string.IsNullOrWhiteSpace(dto.Title)
                && string.IsNullOrWhiteSpace(dto.Description)
                && !dto.Severity.HasValue
                && !dto.AssignedTo.HasValue;

            if (!isUpdatingStatusOnly && !canEditAny)
            {
                if (!canEditOwn || currentIncident.CreatedBy != performedByUserId)
                {
                    return Forbid();
                }
            }

            if (dto.AssignedTo.HasValue && !User.HasClaim("permission", PermissionConstants.IncidentsAssign))
            {
                return Forbid();
            }

            if (dto.Severity.HasValue && !User.HasClaim("permission", PermissionConstants.IncidentsSeverityAny))
            {
                return Forbid();
            }

            if (dto.Status.HasValue)
            {
                var canSetAnyStatus = User.HasClaim("permission", PermissionConstants.IncidentsStatusAny);
                if (!canSetAnyStatus)
                {
                    var canSetLimitedStatus = User.HasClaim("permission", PermissionConstants.IncidentsStatusLimited);

                    if (!canSetLimitedStatus || !isOwnedIncident)
                    {
                        return Forbid();
                    }
                }
            }

            var incident = await _mediator.Send(new UpdateIncidentCommand
            {
                Id = id,
                PerformedByUserId = performedByUserId,
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
        [Authorize(Policy = PolicyConstants.CanDeleteIncidents)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var performedByUserId))
            {
                return Unauthorized("Missing authenticated user context.");
            }

            await _mediator.Send(new DeleteIncidentCommand(id, performedByUserId));
            return NoContent();
        }

        [Authorize(Policy = PolicyConstants.CanRestoreIncidents)]
        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var performedByUserId))
            {
                return Unauthorized("Missing authenticated user context.");
            }

            var restored = await _mediator.Send(new RestoreIncidentCommand(id, performedByUserId));
            if (!restored)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}