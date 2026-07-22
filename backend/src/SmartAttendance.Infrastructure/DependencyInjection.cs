using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Infrastructure.Persistence;
using SmartAttendance.Infrastructure.Persistence.Interceptors;

namespace SmartAttendance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ──────────────────────────────────────────────
        // Interceptors (registered as scoped for per-request lifecycle)
        // ──────────────────────────────────────────────
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DomainEventDispatcherInterceptor>();

        // ──────────────────────────────────────────────
        // DbContext — SQL Server
        // ──────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            // Attach interceptors from the DI container
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<DomainEventDispatcherInterceptor>());

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.CommandTimeout(30);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqlOptions.MinBatchSize(2);
                sqlOptions.MaxBatchSize(100);
            });

            // Suppress the pending model changes warning since we are not auto-generating migrations
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // ──────────────────────────────────────────────
        // Interface registrations
        // ──────────────────────────────────────────────
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<SmartAttendance.Infrastructure.Persistence.Initialization.IApplicationDbContextInitializer, SmartAttendance.Infrastructure.Persistence.Initialization.ApplicationDbContextInitializer>();

        // ──────────────────────────────────────────────
        // JWT Authentication (existing — untouched per constraints)
        // ──────────────────────────────────────────────
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings.GetValue<string>("Secret");
        
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret is missing from configuration.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.GetValue<string>("Issuer") ?? "SmartAttendance",
                ValidAudience = jwtSettings.GetValue<string>("Audience") ?? "SmartAttendanceClient",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // ──────────────────────────────────────────────
        // Identity
        // ──────────────────────────────────────────────
        services.AddIdentityCore<SmartAttendance.Infrastructure.Identity.Models.ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 12;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddRoles<SmartAttendance.Infrastructure.Identity.Models.ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ──────────────────────────────────────────────
        // Authorization & Security Services
        // ──────────────────────────────────────────────
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, SmartAttendance.Infrastructure.Identity.Authorization.PermissionPolicyProvider>();
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, SmartAttendance.Infrastructure.Identity.Authorization.PermissionAuthorizationHandler>();
        
        services.AddAuthorization();

        // ──────────────────────────────────────────────
        // Application Services & Providers
        // ──────────────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, SmartAttendance.Infrastructure.Services.CurrentUserService>();
        services.AddScoped<SmartAttendance.Infrastructure.Identity.Services.IJwtTokenGenerator, SmartAttendance.Infrastructure.Identity.Services.JwtTokenGenerator>();
        services.AddScoped<SmartAttendance.Application.Common.Interfaces.IAuthenticationService, SmartAttendance.Infrastructure.Identity.Services.AuthenticationService>();
        services.AddScoped<IIdentityService, SmartAttendance.Infrastructure.Identity.Services.IdentityService>();

        // ──────────────────────────────────────────────
        // Options Configurations
        // ──────────────────────────────────────────────
        services.Configure<SmartAttendance.Infrastructure.Options.JwtOptions>(configuration.GetSection("JwtSettings"));
        services.Configure<SmartAttendance.Infrastructure.Options.EmailOptions>(configuration.GetSection("EmailSettings"));
        services.Configure<SmartAttendance.Infrastructure.Options.SmsOptions>(configuration.GetSection("SmsSettings"));
        services.Configure<SmartAttendance.Infrastructure.Options.StorageOptions>(configuration.GetSection("StorageSettings"));
        services.Configure<SmartAttendance.Infrastructure.Options.FaceRecognitionOptions>(configuration.GetSection("FaceRecognitionSettings"));
        services.Configure<SmartAttendance.Infrastructure.Options.GeoLocationOptions>(configuration.GetSection("GeoLocationSettings"));

        // ──────────────────────────────────────────────
        // Caching
        // ──────────────────────────────────────────────
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, SmartAttendance.Infrastructure.Caching.MemoryCacheService>();

        // ──────────────────────────────────────────────
        // Infrastructure Services
        // ──────────────────────────────────────────────
        services.AddSingleton<IDateTimeProvider, SmartAttendance.Infrastructure.Services.DateTimeProvider>();
        services.AddScoped<SmartAttendance.Infrastructure.Services.GeoLocation.Providers.IGeoLocationProvider, SmartAttendance.Infrastructure.Services.GeoLocation.Providers.NullGeoLocationProvider>();
        services.AddScoped<IGeoLocationService, SmartAttendance.Infrastructure.Services.GeoLocation.GeoLocationService>();
        
        services.AddScoped<SmartAttendance.Infrastructure.Services.ICurrentTenantService, SmartAttendance.Infrastructure.Services.CurrentTenantService>();
        services.AddScoped<SmartAttendance.Infrastructure.Services.FaceRecognition.Providers.IFaceRecognitionProvider, SmartAttendance.Infrastructure.Services.FaceRecognition.Providers.NullFaceRecognitionProvider>();
        services.AddScoped<IFaceRecognitionService, SmartAttendance.Infrastructure.Services.FaceRecognition.FaceRecognitionService>();
        services.AddScoped<IFileStorageService, SmartAttendance.Infrastructure.Services.FileStorage.FileStorageService>();
        
        // Notifications
        services.AddScoped<IEmailService, SmartAttendance.Infrastructure.Services.Email.EmailService>();
        services.AddScoped<SmartAttendance.Infrastructure.Services.SMS.ISmsService, SmartAttendance.Infrastructure.Services.SMS.SmsService>();
        services.AddScoped<SmartAttendance.Infrastructure.Services.Notification.Providers.INotificationProvider, SmartAttendance.Infrastructure.Services.Notification.Providers.NullNotificationProvider>();
        services.AddScoped<INotificationService, SmartAttendance.Infrastructure.Services.Notifications.NotificationDispatcher>();

        // Background Jobs
        services.AddScoped<SmartAttendance.Infrastructure.BackgroundJobs.IBackgroundJobScheduler, SmartAttendance.Infrastructure.BackgroundJobs.BackgroundJobScheduler>();

        return services;
    }
}
