using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SmartAttendance.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access.");
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status401Unauthorized, "Unauthorized");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Bad request argument.");
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status400BadRequest, "Bad Request");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict occurred.");
            await HandleExceptionAsync(context, "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.", StatusCodes.Status409Conflict, "Concurrency Conflict");
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error.");
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

            var problemDetails = new HttpValidationProblemDetails(ex.Errors.GroupBy(e => e.PropertyName, e => e.ErrorMessage).ToDictionary(g => g.Key, g => g.ToArray()))
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                Instance = context.Request.Path
            };
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            var isDevelopment = _env.EnvironmentName == "Development";
            var detail = isDevelopment ? ex.Message : "An unexpected error occurred.";
            await HandleExceptionAsync(context, detail, StatusCodes.Status500InternalServerError, "Internal Server Error");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, string detail, int statusCode, string title)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
