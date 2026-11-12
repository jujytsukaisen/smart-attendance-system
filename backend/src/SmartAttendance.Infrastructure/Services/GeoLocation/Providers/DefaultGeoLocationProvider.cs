using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.GeoLocation.Providers;

/// <summary>
/// Default implementation of IGeoLocationProvider.
/// The actual distance check is already performed in GeoLocationService using
/// the Haversine formula before this provider is called. If execution reaches
/// here, the distance validation has already passed — so we return Success.
/// 
/// Replace this with a real external provider (Google Maps, HERE, etc.)
/// if additional third-party geofencing verification is required.
/// </summary>
public class DefaultGeoLocationProvider : IGeoLocationProvider
{
    public Task<Result<bool>> ValidateLocationAsync(
        double latitude, 
        double longitude, 
        double targetLatitude, 
        double targetLongitude, 
        double radiusMeters)
    {
        // الوصول إلى هنا يعني فحص المسافة (Haversine) نجح بالفعل في GeoLocationService
        // لا حاجة لتحقق إضافي — يمكن استبدال هذا بـ provider خارجي مستقبلاً
        return Task.FromResult(Result<bool>.Success(true));
    }
}
