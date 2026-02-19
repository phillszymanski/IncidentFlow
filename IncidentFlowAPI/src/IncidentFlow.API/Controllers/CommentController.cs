using IncidentFlow.API.Contracts.Comments;
using IncidentFlow.API.Authorization;
using IncidentFlow.Application.Features.Comments.Commands;
using IncidentFlow.Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : BaseController
{
    private readonly IMediator _mediator;

    public CommentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Policy = "CanReadIncidents")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? incidentId)
    {
        var comments = incidentId.HasValue
            ? await _mediator.Send(new GetCommentsByIncidentIdQuery(incidentId.Value))
            : await _mediator.Send(new GetAllCommentsQuery());

        var dto = comments.Select(x => x.ToResponseDto());
        return HandleResult(dto);
    }

    [Authorize(Policy = "CanReadIncidents")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var comment = await _mediator.Send(new GetCommentByIdQuery(id));
        if (comment is null) return NotFound();

        return HandleResult(comment.ToResponseDto());
    }

    [Authorize(Policy = "CanWriteIncidents")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var createdByUserId))
        {
            return Unauthorized("Missing authenticated user context.");
        }

        var id = await _mediator.Send(new CreateCommentCommand
        {
            IncidentId = dto.IncidentId,
            Content = dto.Content,
            CreatedByUserId = createdByUserId
        });

        var created = await _mediator.Send(new GetCommentByIdQuery(id));
        return CreatedAtAction(nameof(Get), new { id }, created?.ToResponseDto());
    }

    [Authorize(Policy = "CanWriteIncidents")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CommentUpdateDto dto)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var performedByUserId))
        {
            return Unauthorized("Missing authenticated user context.");
        }

        var existingComment = await _mediator.Send(new GetCommentByIdQuery(id));
        if (existingComment is null)
        {
            return NotFound();
        }

        var canEditAny = User.HasClaim("permission", PermissionConstants.IncidentsEditAny);
        if (!canEditAny && existingComment.CreatedByUserId != performedByUserId)
        {
            return Forbid();
        }

        var comment = await _mediator.Send(new UpdateCommentCommand
        {
            Id = id,
            Content = dto.Content
        });

        if (comment is null) return NotFound();
        return NoContent();
    }

    [Authorize(Policy = "CanWriteIncidents")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var performedByUserId))
        {
            return Unauthorized("Missing authenticated user context.");
        }

        var existingComment = await _mediator.Send(new GetCommentByIdQuery(id));
        if (existingComment is null)
        {
            return NotFound();
        }

        var canEditAny = User.HasClaim("permission", PermissionConstants.IncidentsEditAny);
        if (!canEditAny && existingComment.CreatedByUserId != performedByUserId)
        {
            return Forbid();
        }

        await _mediator.Send(new DeleteCommentCommand(id));
        return NoContent();
    }
}
