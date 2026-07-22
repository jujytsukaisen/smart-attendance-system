using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidNotificationException : DomainException
{
    public InvalidNotificationException(string message) : base(message)
    {
    }
}
