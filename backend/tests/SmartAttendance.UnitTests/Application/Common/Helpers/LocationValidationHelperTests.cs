using System;
using SmartAttendance.Application.Common.Helpers;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.ValueObjects;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Common.Helpers;

public class LocationValidationHelperTests
{
    private readonly Guid _companyId = Guid.NewGuid();

    private Branch CreateTestBranch(double lat = 31.996, double lon = 44.314, double radius = 50)
    {
        var address = new Address("Street 1", "Baghdad", "State", "Iraq", "10001");
        var phone = new PhoneNumber("+9647700000000");
        var email = new Email("branch@company.com");

        return Branch.Create(
            _companyId,
            "الفرع الرئيسي",
            "HQ-01",
            address,
            phone,
            email,
            lat,
            lon,
            radius,
            "Asia/Baghdad",
            true);
    }

    [Fact]
    public void ValidateLocation_WhenUserWithinBranchRadius_ReturnsSuccess()
    {
        // Branch at (31.996000, 44.314000) with 50m radius
        var branch = CreateTestBranch(31.996000, 44.314000, 50);

        // User is very close (~10m away)
        double userLat = 31.996050;
        double userLon = 44.314050;

        var result = LocationValidationHelper.ValidateLocation(userLat, userLon, branch, null, null);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void ValidateLocation_WhenUserKilometersAwayFromBranch_ReturnsFailure()
    {
        // Branch at (31.996000, 44.314000) with 50m radius (user coordinates from screenshot)
        var branch = CreateTestBranch(31.996000, 44.314000, 50);

        // User is in another city/area (~150km away, e.g. Baghdad 33.3152, 44.3661)
        double userLat = 33.3152;
        double userLon = 44.3661;

        var result = LocationValidationHelper.ValidateLocation(userLat, userLon, branch, null, null);

        Assert.False(result.Succeeded);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("تم رفض تسجيل الحضور", result.Errors[0]);
    }

    [Fact]
    public void ValidateLocation_WhenCoordinatesAreZeroAndBranchHasGeofence_ReturnsFailure()
    {
        var branch = CreateTestBranch(31.996000, 44.314000, 50);

        // User sent 0, 0 (GPS disabled or blocked)
        double userLat = 0;
        double userLon = 0;

        var result = LocationValidationHelper.ValidateLocation(userLat, userLon, branch, null, null);

        Assert.False(result.Succeeded);
        Assert.Contains("GPS", result.Errors[0]);
    }

    [Fact]
    public void ValidateLocation_WhenNoGeofenceConfiguredAndGpsNotRequired_ReturnsSuccess()
    {
        // No branch and no device
        var result = LocationValidationHelper.ValidateLocation(0, 0, null, null, null);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void CalculateDistanceMeters_ComputesAccurateDistance()
    {
        // Distance between Baghdad (33.3152, 44.3661) and Najaf (31.9960, 44.3140) is ~147km
        double dist = LocationValidationHelper.CalculateDistanceMeters(33.3152, 44.3661, 31.9960, 44.3140);

        Assert.InRange(dist, 140000, 160000);
    }
}
