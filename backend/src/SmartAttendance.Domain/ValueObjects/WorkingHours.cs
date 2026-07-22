using System;
using System.Collections.Generic;
using SmartAttendance.Domain.Common;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.ValueObjects;

public class WorkingHours : ValueObject
{
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    public WorkingHours(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime == endTime)
        {
            throw new InvalidWorkingHoursException("Start time and end time cannot be the same.");
        }

        StartTime = startTime;
        EndTime = endTime;
    }

    public TimeSpan Duration
    {
        get
        {
            if (EndTime > StartTime)
            {
                return EndTime - StartTime;
            }
            
            // Overnight shift calculation (e.g. 22:00 to 06:00)
            return (TimeSpan.FromDays(1) - StartTime.ToTimeSpan()) + EndTime.ToTimeSpan();
        }
    }

    public bool IsOvernight => StartTime > EndTime;

    public bool OverlapsWith(WorkingHours other)
    {
        if (other == null) return false;

        return Contains(other.StartTime) || Contains(other.EndTime) || 
               other.Contains(StartTime) || other.Contains(EndTime);
    }

    public bool Contains(TimeOnly time)
    {
        if (!IsOvernight)
        {
            return time >= StartTime && time <= EndTime;
        }

        // Overnight shift contains times from StartTime to Midnight OR from Midnight to EndTime
        return time >= StartTime || time <= EndTime;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
    }

    public override string ToString() => $"{StartTime} to {EndTime} (Duration: {Duration})";
}
