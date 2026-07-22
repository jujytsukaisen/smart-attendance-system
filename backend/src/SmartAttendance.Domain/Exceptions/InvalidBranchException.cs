using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidBranchException : DomainException
{
    public InvalidBranchException(string message) : base(message)
    {
    }
}
