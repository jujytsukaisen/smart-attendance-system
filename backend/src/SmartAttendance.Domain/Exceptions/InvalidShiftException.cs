using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidShiftException : DomainException
{
    public InvalidShiftException(string message) : base(message)
    {
    }
}
