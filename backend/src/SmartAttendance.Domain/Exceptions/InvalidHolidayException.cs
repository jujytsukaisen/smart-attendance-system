using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidHolidayException : DomainException
{
    public InvalidHolidayException(string message) : base(message)
    {
    }
}
