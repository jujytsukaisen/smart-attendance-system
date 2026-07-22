using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidBreakException : DomainException
{
    public InvalidBreakException(string message) : base(message)
    {
    }
}
