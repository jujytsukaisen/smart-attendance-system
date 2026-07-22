using System;
using SmartAttendance.Application.Common.Interfaces;

namespace SmartAttendance.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}
