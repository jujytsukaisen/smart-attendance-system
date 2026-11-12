using System;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Common.Helpers;

/// <summary>
/// Helper for validating employee physical presence against Branch or Device geofencing boundaries.
/// </summary>
public static class LocationValidationHelper
{
    private const double EarthRadiusMeters = 6371000.0;

    /// <summary>
    /// Validates whether the user's provided GPS coordinates fall within the allowable geofenced radius
    /// of the employee's assigned Branch or the registered AttendanceDevice.
    /// </summary>
    public static Result ValidateLocation(
        double userLatitude,
        double userLongitude,
        Branch? branch,
        AttendanceDevice? device,
        AttendancePolicy? policy,
        bool departmentRequiresLocation = true)
    {
        bool branchHasCoordinates = branch != null && branch.Latitude.HasValue && branch.Longitude.HasValue &&
            (Math.Abs(branch.Latitude.Value) > 0.00001 || Math.Abs(branch.Longitude.Value) > 0.00001);

        bool deviceHasCoordinates = device != null && device.Latitude.HasValue && device.Longitude.HasValue &&
            (Math.Abs(device.Latitude.Value) > 0.00001 || Math.Abs(device.Longitude.Value) > 0.00001);

        bool isGpsRequiredByPolicy = policy?.RequireGPS == true;
        bool isGpsRequiredByBranch = branchHasCoordinates && branch!.RadiusInMeters.GetValueOrDefault() > 0;

        // If the employee's department explicitly disabled location validation, bypass entirely
        if (!departmentRequiresLocation)
        {
            return Result.Success();
        }

        // If GPS is neither required by policy nor enabled on the branch, bypass validation
        if (!isGpsRequiredByPolicy && !isGpsRequiredByBranch && !deviceHasCoordinates)
        {
            return Result.Success();
        }

        // Determine target location and allowed radius
        double targetLat;
        double targetLon;
        double allowedRadius;
        string locationName;

        if (branchHasCoordinates)
        {
            targetLat = branch!.Latitude!.Value;
            targetLon = branch.Longitude!.Value;
            allowedRadius = branch.RadiusInMeters.GetValueOrDefault() > 0 
                ? branch.RadiusInMeters!.Value 
                : (policy?.AllowedRadiusMeters > 0 ? policy.AllowedRadiusMeters : 50);
            locationName = string.IsNullOrWhiteSpace(branch.Name) ? "الفرع" : branch.Name;
        }
        else if (deviceHasCoordinates)
        {
            targetLat = device!.Latitude!.Value;
            targetLon = device.Longitude!.Value;
            allowedRadius = device.AllowedRadiusMeters.GetValueOrDefault() > 0 
                ? device.AllowedRadiusMeters!.Value 
                : (policy?.AllowedRadiusMeters > 0 ? policy.AllowedRadiusMeters : 50);
            locationName = string.IsNullOrWhiteSpace(device.Name) ? "جهاز الحضور" : device.Name;
        }
        else
        {
            // If GPS is required by policy but no target coordinates are set on branch or device
            if (isGpsRequiredByPolicy && policy?.AllowedRadiusMeters > 0 && policy.MaximumDistanceMeters > 0)
            {
                // No reference coordinates configured in system
                return Result.Success();
            }
            return Result.Success();
        }

        // Verify that user coordinates were provided
        if (Math.Abs(userLatitude) < 0.00001 && Math.Abs(userLongitude) < 0.00001)
        {
            return Result.Failure(new[]
            {
                $"مطلوب تفعيل وتحديد الموقع الجغرافي (GPS) لتسجيل الحضور في ({locationName}). يرجى منح إذن الوصول للموقع في المتصفح.",
                $"GPS location is required for attendance at ({locationName}). Please grant location permission."
            });
        }

        // Calculate distance
        double distance = CalculateDistanceMeters(userLatitude, userLongitude, targetLat, targetLon);

        if (distance > allowedRadius)
        {
            string distanceText = FormatDistance(distance);
            string radiusText = FormatDistance(allowedRadius);

            return Result.Failure(new[]
            {
                $"أنت تبعد {distanceText} عن موقع ({locationName})، بينما النطاق المسموح به هو {radiusText}. تم رفض تسجيل الحضور لعدم التواجد في المكان المحدد.",
                $"You are {distanceText} away from ({locationName}), exceeding the allowed radius of {radiusText}. Attendance rejected."
            });
        }

        return Result.Success();
    }

    /// <summary>
    /// Calculates the great-circle distance between two geographic coordinates using the Haversine formula.
    /// </summary>
    public static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var rLat1 = ToRadians(lat1);
        var rLat2 = ToRadians(lat2);

        var a = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) +
                Math.Cos(rLat1) * Math.Cos(rLat2) *
                Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0);

        var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

        return EarthRadiusMeters * c;
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);

    private static string FormatDistance(double meters)
    {
        if (meters >= 1000)
        {
            return $"{(meters / 1000.0):0.#} كم";
        }
        return $"{Math.Round(meters)} متر";
    }
}
