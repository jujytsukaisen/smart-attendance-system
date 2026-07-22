using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidAttendancePolicyException : DomainException
{
    public InvalidAttendancePolicyException(string message) : base(message)
    {
    }
}
