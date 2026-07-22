using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidAttendanceException : DomainException
{
    public InvalidAttendanceException(string message) : base(message)
    {
    }
}
