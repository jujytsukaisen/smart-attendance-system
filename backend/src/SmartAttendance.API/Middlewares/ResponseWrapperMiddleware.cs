using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SmartAttendance.API.Middlewares;

public class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseWrapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ignore response wrapper for Swagger or Health endpoints
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var readToEnd = await new StreamReader(memoryStream).ReadToEndAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Don't wrap already formatted problem details or 204 No Content
            if (context.Response.ContentType?.Contains("problem+json") == true || context.Response.StatusCode == StatusCodes.Status204NoContent)
            {
                await memoryStream.CopyToAsync(originalBodyStream);
                return;
            }

            var wrappedResponse = new
            {
                data = string.IsNullOrWhiteSpace(readToEnd) ? null : JsonSerializer.Deserialize<object>(readToEnd),
                statusCode = context.Response.StatusCode,
                isSuccess = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300
            };

            var json = JsonSerializer.Serialize(wrappedResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            context.Response.Body = originalBodyStream;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}
