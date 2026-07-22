using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Infrastructure.Persistence;
using SmartAttendance.Infrastructure.Persistence.Seed;

namespace SmartAttendance.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "this_is_a_very_long_test_secret_for_integration_tests");
    }

    public Mock<IEmailService> EmailServiceMock { get; } = new();
    public Mock<INotificationService> NotificationServiceMock { get; } = new();
    public Mock<IFaceRecognitionService> FaceRecognitionServiceMock { get; } = new();
    public Mock<IGeoLocationService> GeoLocationServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("JwtSettings:Secret", "this_is_a_very_long_test_secret_for_integration_tests")
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove all Entity Framework Core services
            var descriptors = services.Where(d => 
                d.ServiceType.Namespace != null && 
                d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore")).ToList();

            foreach (var d in descriptors)
            {
                services.Remove(d);
            }

            // Also remove DbConnection if registered
            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
            if (dbConnectionDescriptor != null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            // Create open SqliteConnection so in-memory database doesn't close for this instance
            var dbName = Guid.NewGuid().ToString();
            _connection = new SqliteConnection($"DataSource={dbName};Mode=Memory;Cache=Shared");
            _connection.Open();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(
                    sp.GetRequiredService<SmartAttendance.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor>(),
                    sp.GetRequiredService<SmartAttendance.Infrastructure.Persistence.Interceptors.DomainEventDispatcherInterceptor>());
                options.UseSqlite(_connection);
            });

            // Replace external services with Mocks
            services.Replace(ServiceDescriptor.Transient<IEmailService>(_ => EmailServiceMock.Object));
            services.Replace(ServiceDescriptor.Transient<INotificationService>(_ => NotificationServiceMock.Object));
            services.Replace(ServiceDescriptor.Transient<IFaceRecognitionService>(_ => FaceRecognitionServiceMock.Object));
            services.Replace(ServiceDescriptor.Transient<IGeoLocationService>(_ => GeoLocationServiceMock.Object));

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;

            // Initialize using the initializer
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close();
        _connection?.Dispose();
    }
}
