using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidAuditLogException : DomainException
{
    public InvalidAuditLogException(string message) : base(message)
    {
    }
}
