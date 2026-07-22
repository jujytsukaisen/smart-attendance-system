using Microsoft.Extensions.DependencyInjection;

namespace SmartAttendance.Application.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Custom application-specific service extensions can be configured here.
        return services;
    }
}
