using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Common.Interfaces;

public interface IGeoLocationService
{
    Task<Result<bool>> ValidateLocationAsync(double latitude, double longitude, double targetLatitude, double targetLongitude, double radiusMeters);
}
