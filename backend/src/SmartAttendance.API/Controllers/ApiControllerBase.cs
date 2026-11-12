using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("Authenticated")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender _mediator = null!;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.Succeeded)
        {
            if (result.Data == null)
            {
                return NoContent();
            }
            return Ok(result.Data);
        }

        return ProblemDetailsForFailure(result);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.Succeeded)
        {
            return NoContent();
        }

        return ProblemDetailsForFailure(result);
    }

    protected IActionResult ProblemDetailsForFailure(Result result)
    {
        // Simple mapping based on error signatures or custom logic can go here.
        // E.g., if Errors contains "NotFound", return 404.
        
        var firstError = result.Errors?.Length > 0 ? result.Errors[0] : "An error occurred";
        
        if (firstError.Contains("not found", System.StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = firstError
            });
        }

        if (firstError.Contains("unauthorized", System.StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = firstError
            });
        }

        if (firstError.Contains("forbidden", System.StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(403, new ProblemDetails
            {
                Status = 403,
                Title = "Forbidden",
                Detail = firstError
            });
        }

        if (firstError.Contains("conflict", System.StringComparison.OrdinalIgnoreCase) || 
            firstError.Contains("already exists", System.StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = firstError
            });
        }

        // Default to Validation Problem (422) if not mapped above
        var problemDetails = new ValidationProblemDetails
        {
            Status = 422,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred."
        };
        problemDetails.Extensions.Add("errors", result.Errors);

        return UnprocessableEntity(problemDetails);
    }
}
