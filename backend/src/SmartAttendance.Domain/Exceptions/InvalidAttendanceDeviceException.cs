using System;

namespace SmartAttendance.Domain.Exceptions;

public class InvalidAttendanceDeviceException : DomainException
{
    public InvalidAttendanceDeviceException(string message) : base(message)
    {
    }
}
