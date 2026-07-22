using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidWorkScheduleException : DomainException
{
    public InvalidWorkScheduleException(string message) : base(message)
    {
    }
}
