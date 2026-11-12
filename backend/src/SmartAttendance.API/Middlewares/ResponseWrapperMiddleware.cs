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

            // Don't wrap already formatted problem details, 204 No Content, or File/Binary downloads
            var contentType = context.Response.ContentType?.ToLowerInvariant() ?? "";
            var isFileOrNonJson = context.Response.Headers.ContainsKey("Content-Disposition") ||
                                  contentType.StartsWith("text/csv") ||
                                  contentType.StartsWith("application/vnd.ms-excel") ||
                                  contentType.StartsWith("application/pdf") ||
                                  contentType.StartsWith("text/html") ||
                                  contentType.StartsWith("application/octet-stream") ||
                                  contentType.StartsWith("image/") ||
                                  contentType.StartsWith("video/") ||
                                  contentType.StartsWith("audio/");

            if (contentType.Contains("problem+json") ||
                context.Response.StatusCode == StatusCodes.Status204NoContent ||
                isFileOrNonJson)
            {
                await memoryStream.CopyToAsync(originalBodyStream);
                return;
            }

           object? responseData = null;

if (!string.IsNullOrWhiteSpace(readToEnd))
{
    try
    {
        responseData = JsonSerializer.Deserialize<object>(readToEnd);
    }
    catch
    {
        responseData = readToEnd;
    }
}

var wrappedResponse = new
{
    data = responseData,
    statusCode = context.Response.StatusCode,
    isSuccess = context.Response.StatusCode >= 200 &&
                context.Response.StatusCode < 300
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
