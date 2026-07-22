using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidLeaveException : DomainException
{
    public InvalidLeaveException(string message) : base(message)
    {
    }
}
