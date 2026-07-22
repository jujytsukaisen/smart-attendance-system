using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidEmployeeException : DomainException
{
    public InvalidEmployeeException(string message) : base(message)
    {
    }
}
