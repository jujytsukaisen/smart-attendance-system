using System;
using System.Collections.Generic;
using SmartAttendance.Domain.Common;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.ValueObjects;

public class TimeRange : ValueObject
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public TimeRange(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            throw new InvalidTimeRangeException($"End time ({end}) must be after start time ({start}).");
        }

        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;

    public bool OverlapsWith(TimeRange other)
    {
        if (other == null) return false;
        return Start < other.End && other.Start < End;
    }

    public bool Contains(DateTime dateTime)
    {
        return dateTime >= Start && dateTime <= End;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start} to {End} (Duration: {Duration})";
}
