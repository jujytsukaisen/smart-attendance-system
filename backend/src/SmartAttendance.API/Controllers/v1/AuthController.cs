using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Application.Features.Authentication.Commands;

namespace SmartAttendance.API.Controllers.v1;

[ApiVersion("1.0")]
public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("Anonymous")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("Anonymous")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}

