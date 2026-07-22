using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.GeoLocation.Providers;

public class NullGeoLocationProvider : IGeoLocationProvider
{
    public Task<Result<bool>> ValidateLocationAsync(double latitude, double longitude, double targetLatitude, double targetLongitude, double radiusMeters)
    {
        return Task.FromResult(Result<bool>.Failure(new[] { "GeoLocation provider is not configured for production." }));
    }
}
