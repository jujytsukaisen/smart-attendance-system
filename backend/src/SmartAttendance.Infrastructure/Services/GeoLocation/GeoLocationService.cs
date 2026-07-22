using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Infrastructure.Options;
using SmartAttendance.Infrastructure.Services.GeoLocation.Providers;

namespace SmartAttendance.Infrastructure.Services.GeoLocation;

public class GeoLocationService : IGeoLocationService
{
    private readonly ILogger<GeoLocationService> _logger;
    private readonly GeoLocationOptions _options;
    private readonly IGeoLocationProvider _provider;

    public GeoLocationService(ILogger<GeoLocationService> logger, IOptions<GeoLocationOptions> options, IGeoLocationProvider provider)
    {
        _logger = logger;
        _options = options.Value;
        _provider = provider;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var r = 6371e3; // Earth radius in meters
        var phi1 = lat1 * Math.PI / 180;
        var phi2 = lat2 * Math.PI / 180;
        var deltaPhi = (lat2 - lat1) * Math.PI / 180;
        var deltaLambda = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return r * c;
    }

    public Task<Result<bool>> ValidateLocationAsync(double latitude, double longitude, double targetLatitude, double targetLongitude, double radiusMeters)
    {
        var distance = CalculateDistance(latitude, longitude, targetLatitude, targetLongitude);
        var allowedRadius = radiusMeters > 0 ? radiusMeters : _options.DefaultRadiusMeters;
        
        _logger.LogInformation("Calculated distance: {Distance}m. Allowed radius: {Radius}m.", distance, allowedRadius);
        
        if (distance > allowedRadius)
        {
            return Task.FromResult(Result<bool>.Failure(new[] { $"User is {Math.Round(distance)}m away, which exceeds the allowed radius of {allowedRadius}m." }));
        }

        return _provider.ValidateLocationAsync(latitude, longitude, targetLatitude, targetLongitude, allowedRadius);
    }
}
