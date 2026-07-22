using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.GeoLocation.Providers;

public interface IGeoLocationProvider
{
    Task<Result<bool>> ValidateLocationAsync(double latitude, double longitude, double targetLatitude, double targetLongitude, double radiusMeters);
}
