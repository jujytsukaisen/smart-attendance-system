using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidDepartmentException : DomainException
{
    public InvalidDepartmentException(string message) : base(message)
    {
    }
}
