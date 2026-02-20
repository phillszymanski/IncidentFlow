using IncidentFlow.API.Contracts.Auth;
using IncidentFlow.API.Services;
using IncidentFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Username/email and password are required.");
        }

        var user = dto.UsernameOrEmail.Contains('@')
            ? await _userRepository.GetByEmailAsync(dto.UsernameOrEmail.Trim(), cancellationToken)
            : await _userRepository.GetByUsernameAsync(dto.UsernameOrEmail.Trim(), cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash) || !_passwordHashService.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials.");
        }

        var token = _jwtTokenService.CreateToken(user);
        var csrfToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var isHttps = Request.IsHttps;

        Response.Cookies.Append(_jwtOptions.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            IsEssential = true
        });

        Response.Cookies.Append(_jwtOptions.CsrfCookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes),
            IsEssential = true
        });

        Response.Headers[_jwtOptions.CsrfHeaderName] = csrfToken;

        return Ok(new LoginResponseDto
        {
            User = new AuthUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            }
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var isHttps = Request.IsHttps;

        Response.Cookies.Delete(_jwtOptions.CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            IsEssential = true
        });

        Response.Cookies.Delete(_jwtOptions.CsrfCookieName, new CookieOptions
        {
            HttpOnly = false,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
            IsEssential = true
        });

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirst("userId")?.Value,
            Username = User.Identity?.Name ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName)?.Value,
            Email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value,
            Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
            Permissions = User.Claims.Where(c => c.Type == "permission").Select(c => c.Value)
        });
    }
}
