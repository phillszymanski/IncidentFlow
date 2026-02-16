using IncidentFlow.API.Contracts.Users;
using IncidentFlow.API.Authorization;
using IncidentFlow.API.Services;
using IncidentFlow.Application.Features.Users.Commands;
using IncidentFlow.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IPasswordHashService _passwordHashService;

    public UserController(IMediator mediator, IPasswordHashService passwordHashService)
    {
        _mediator = mediator;
        _passwordHashService = passwordHashService;
    }

    [Authorize(Policy = PolicyConstants.CanManageUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _mediator.Send(new GetAllUsersQuery());
        var dto = users.Select(x => x.ToResponseDto());
        return HandleResult(dto);
    }

    [Authorize(Policy = PolicyConstants.CanAssignIncidents)]
    [HttpGet("assignable")]
    public async Task<IActionResult> GetAssignable()
    {
        var users = await _mediator.Send(new GetAllUsersQuery());
        var dto = users.Select(x => x.ToResponseDto());
        return HandleResult(dto);
    }

    [Authorize(Policy = PolicyConstants.CanReadIncidents)]
    [HttpGet("directory")]
    public async Task<IActionResult> GetDirectory()
    {
        var users = await _mediator.Send(new GetAllUsersQuery());
        var dto = users.Select(x => x.ToResponseDto());
        return HandleResult(dto);
    }

    [Authorize(Policy = PolicyConstants.CanManageUsers)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id));
        if (user is null) return NotFound();

        return HandleResult(user.ToResponseDto());
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Password is required.");

        var requestedRole = dto.Role;
        var canManageUsers = User.HasClaim("permission", PermissionConstants.UsersManage);
        var effectiveRole = canManageUsers ? requestedRole : "User";

        var id = await _mediator.Send(new CreateUserCommand
        {
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            Role = effectiveRole,
            PasswordHash = _passwordHashService.HashPassword(dto.Password)
        });

        var created = await _mediator.Send(new GetUserByIdQuery(id));
        return CreatedAtAction(nameof(Get), new { id }, created?.ToResponseDto());
    }

    [Authorize(Policy = PolicyConstants.CanManageUsers)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateDto dto)
    {
        var user = await _mediator.Send(new UpdateUserCommand
        {
            Id = id,
            Username = dto.Username,
            Email = dto.Email,
            FullName = dto.FullName,
            Role = dto.Role,
            PasswordHash = string.IsNullOrWhiteSpace(dto.Password)
                ? null
                : _passwordHashService.HashPassword(dto.Password)
        });

        if (user is null) return NotFound();
        return NoContent();
    }

    [Authorize(Policy = PolicyConstants.CanManageUsers)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}