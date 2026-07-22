using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidUserException : DomainException
{
    public InvalidUserException(string message) : base(message)
    {
    }
}
