using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidPermissionException : DomainException
{
    public InvalidPermissionException(string message) : base(message)
    {
    }
}
