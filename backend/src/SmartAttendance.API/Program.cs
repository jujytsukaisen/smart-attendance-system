using System;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartAttendance.API.Middlewares;
using SmartAttendance.Application;
using SmartAttendance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// 1. Serilog Configuration
// ──────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ──────────────────────────────────────────────
// 2. Add Services (Layers & Configurations)
// ──────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ──────────────────────────────────────────────
// 3. Controllers & JSON Behavior
// ──────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    // Minimal standard options can be added here
})
.AddJsonOptions(options =>
{
    // Enum as strings
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new SmartAttendance.API.Converters.UtcDateTimeJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new SmartAttendance.API.Converters.NullableUtcDateTimeJsonConverter());
    // Ignore null values
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    // CamelCase JSON is the default in ASP.NET Core
});

// Suppress the default model state validation to use our custom ProblemDetails logic if desired
// builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

// ──────────────────────────────────────────────
// 4. API Versioning
// ──────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ──────────────────────────────────────────────
// 5. Rate Limiting
// ──────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Anonymous", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 30;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("Authenticated", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("Admin", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 500;
        opt.QueueLimit = 0;
    });
});

// ──────────────────────────────────────────────
// 6. CORS
// ──────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ──────────────────────────────────────────────
// 7. Response Compression
// ──────────────────────────────────────────────
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// ──────────────────────────────────────────────
// 8. Output Caching
// ──────────────────────────────────────────────
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
    options.AddPolicy("StandardCache", builder => builder.Expire(TimeSpan.FromSeconds(60)));
});

// ──────────────────────────────────────────────
// 9. Health Checks
// ──────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SmartAttendance.Infrastructure.Persistence.ApplicationDbContext>("SQL Server")
    .AddCheck("Application", () => HealthCheckResult.Healthy("Running"));

// ──────────────────────────────────────────────
// 10. Swagger Configuration
// ──────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Attendance API",
        Version = "v1",
        Description = "Enterprise Web API for the Smart Attendance System"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
});
// ──────────────────────────────────────────────
// BUILD APPLICATION
// ──────────────────────────────────────────────
var app = builder.Build();

// ──────────────────────────────────────────────
// Swagger
// ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Attendance API v1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHsts();
}

// ──────────────────────────────────────────────
// Global Exception Handling
// ──────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

// ──────────────────────────────────────────────
// Logging & Correlation
// ──────────────────────────────────────────────
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// ──────────────────────────────────────────────
// Security Headers
// ──────────────────────────────────────────────
app.UseMiddleware<SecurityHeadersMiddleware>();

// ──────────────────────────────────────────────
// HTTPS
// ──────────────────────────────────────────────
app.UseHttpsRedirection();

// ──────────────────────────────────────────────
// Static Files
// ──────────────────────────────────────────────
app.UseStaticFiles();

// ──────────────────────────────────────────────
// Response Compression & Output Cache
// ──────────────────────────────────────────────
app.UseResponseCompression();
app.UseOutputCache();

// ──────────────────────────────────────────────
// Routing
// ──────────────────────────────────────────────
// ──────────────────────────────────────────────
app.UseRouting();

// ──────────────────────────────────────────────
// CORS
// ──────────────────────────────────────────────
app.UseCors("AllowFrontend");

// ──────────────────────────────────────────────
// Rate Limiter
// ──────────────────────────────────────────────
app.UseRateLimiter();

// ──────────────────────────────────────────────
// Authentication & Authorization
// ──────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ──────────────────────────────────────────────
// Tenant Resolution
// ──────────────────────────────────────────────
app.UseMiddleware<TenantResolutionMiddleware>();

// ──────────────────────────────────────────────
// Response Wrapper
// ──────────────────────────────────────────────
app.UseMiddleware<ResponseWrapperMiddleware>();

// ──────────────────────────────────────────────
// Health Checks
// ──────────────────────────────────────────────
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Name == "Application"
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Name == "SQL Server"
});

// ──────────────────────────────────────────────
// Controllers
// ──────────────────────────────────────────────
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<SmartAttendance.Infrastructure.Persistence.Initialization.IApplicationDbContextInitializer>();
    await initializer.InitializeAsync();
    await initializer.SeedAsync();
}

try
{
    Log.Information("Starting Web Host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
