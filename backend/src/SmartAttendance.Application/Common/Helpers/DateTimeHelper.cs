using System;

namespace SmartAttendance.Application.Common.Helpers;

public static class DateTimeHelper
{
    public static DateTime GetStartOfDay(this DateTime dateTime) => dateTime.Date;
    public static DateTime GetEndOfDay(this DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);
}
