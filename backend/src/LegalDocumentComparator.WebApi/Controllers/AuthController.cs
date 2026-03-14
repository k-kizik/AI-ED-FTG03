using LegalDocumentComparator.Application.UseCases.Auth.Login;
using LegalDocumentComparator.Application.UseCases.Auth.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocumentComparator.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _loginHandler;
    private readonly RegisterCommandHandler _registerHandler;

    public AuthController(
        LoginCommandHandler loginHandler,
        RegisterCommandHandler registerHandler)
    {
        _loginHandler = loginHandler;
        _registerHandler = registerHandler;
    }

    /// <summary>
    /// Authenticate user and get JWT token
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/login
    ///     {
    ///         "email": "user@legal.com",
    ///         "password": "User123!"
    ///     }
    /// 
    /// Default test accounts:
    /// - User: user@legal.com / User123!
    /// - Manager: admin@legal.com / Admin123!
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginCommand command)
    {
        var result = await _loginHandler.Handle(command, CancellationToken.None);
        return Ok(result);
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="command">Registration details</param>
    /// <returns>Created user information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/auth/register
    ///     {
    ///         "email": "newuser@example.com",
    ///         "password": "SecurePass123!",
    ///         "confirmPassword": "SecurePass123!"
    ///     }
    /// 
    /// Password requirements:
    /// - At least 8 characters
    /// - Contains uppercase and lowercase letters
    /// - Contains at least one digit
    /// - Contains at least one special character
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResult>> Register([FromBody] RegisterCommand command)
    {
        var result = await _registerHandler.Handle(command, CancellationToken.None);
        return Ok(result);
    }
}

