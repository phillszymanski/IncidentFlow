using IncidentFlow.API.Contracts.Users;
using IncidentFlow.Application.Features.Users.Commands;
using IncidentFlow.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _mediator.Send(new GetAllUsersQuery());
        var dto = users.Select(x => x.ToResponseDto());
        return HandleResult(dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id));
        if (user is null) return NotFound();

        return HandleResult(user.ToResponseDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _mediator.Send(new CreateUserCommand
        {
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName
        });

        var created = await _mediator.Send(new GetUserByIdQuery(id));
        return CreatedAtAction(nameof(Get), new { id }, created?.ToResponseDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateDto dto)
    {
        var user = await _mediator.Send(new UpdateUserCommand
        {
            Id = id,
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName
        });

        if (user is null) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}