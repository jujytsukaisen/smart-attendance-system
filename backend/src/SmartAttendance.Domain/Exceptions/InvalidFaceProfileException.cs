using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidFaceProfileException : DomainException
{
    public InvalidFaceProfileException(string message) : base(message)
    {
    }
}
