using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidRoleException : DomainException
{
    public InvalidRoleException(string message) : base(message)
    {
    }
}
