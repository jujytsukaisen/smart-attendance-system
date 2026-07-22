using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidCompanyException : DomainException
{
    public InvalidCompanyException(string message) : base(message)
    {
    }
}
