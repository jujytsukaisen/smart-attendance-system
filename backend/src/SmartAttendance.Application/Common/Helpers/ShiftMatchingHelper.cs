using System;
using System.Collections.Generic;
using System.Linq;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Common.Helpers;

/// <summary>
/// Helper for automatically detecting and resolving the optimal work shift 
/// for an employee based on the exact timestamp of their biometric / attendance scan.
/// </summary>
public static class ShiftMatchingHelper
{
    /// <summary>
    /// Finds the best matching active shift for a given scan timestamp.
    /// Handles standard day shifts, overnight/night shifts crossing midnight, early check-in buffers, and grace periods.
    /// </summary>
    /// <param name="shifts">The candidate shifts (e.g. all shifts for the tenant/company).</param>
    /// <param name="scanTimestamp">The timestamp of the attendance scan.</param>
    /// <returns>The best matching Shift, or null if no shifts exist.</returns>
    public static Shift? DetectBestShift(IEnumerable<Shift>? shifts, DateTime scanTimestamp)
    {
        if (shifts == null) return null;

        var activeShifts = shifts.Where(s => s.IsActive).ToList();
        if (activeShifts.Count == 0) return null;
        if (activeShifts.Count == 1) return activeShifts[0];

        var scanTime = TimeOnly.FromDateTime(scanTimestamp);
        var scanMinutes = scanTime.Hour * 60 + scanTime.Minute;

        var scoredShifts = activeShifts.Select(shift =>
        {
            var startMinutes = shift.StartTime.Hour * 60 + shift.StartTime.Minute;
            var endMinutes = shift.EndTime.Hour * 60 + shift.EndTime.Minute;
            var isNight = shift.IsNightShift || startMinutes > endMinutes;

            var earlyBuffer = Math.Max(0, shift.AllowEarlyCheckInMinutes);
            var lateBuffer = Math.Max(0, shift.AllowLateCheckOutMinutes);

            bool isInWindow;
            if (!isNight)
            {
                var windowStart = (startMinutes - earlyBuffer + 1440) % 1440;
                var windowEnd = endMinutes + lateBuffer;

                if (windowEnd < 1440 && windowStart <= windowEnd)
                {
                    isInWindow = scanMinutes >= windowStart && scanMinutes <= windowEnd;
                }
                else
                {
                    var normalizedEnd = windowEnd % 1440;
                    isInWindow = scanMinutes >= windowStart || scanMinutes <= normalizedEnd;
                }
            }
            else
            {
                // Night shift crosses midnight
                var windowStart = (startMinutes - earlyBuffer + 1440) % 1440;
                var windowEnd = (endMinutes + lateBuffer) % 1440;
                isInWindow = scanMinutes >= windowStart || scanMinutes <= windowEnd;
            }

            // Proximity to shift start time (circular distance in 24 hours: 0 - 720 minutes)
            var diff = Math.Abs(scanMinutes - startMinutes);
            var circularDiff = Math.Min(diff, 1440 - diff);

            // Base score is circular difference
            // In-window shifts receive a massive bonus (-10000) so they always beat out-of-window shifts
            var priorityScore = circularDiff;
            if (isInWindow)
            {
                priorityScore -= 10000;
            }

            return new { Shift = shift, Score = priorityScore };
        })
        .OrderBy(x => x.Score)
        .ToList();

        return scoredShifts.FirstOrDefault()?.Shift;
    }
}
