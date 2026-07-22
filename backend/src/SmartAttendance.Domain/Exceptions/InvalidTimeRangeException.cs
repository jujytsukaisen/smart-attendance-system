namespace SmartAttendance.Domain.Exceptions;

public class InvalidTimeRangeException : DomainException
{
    public InvalidTimeRangeException(string message) : base(message)
    {
    }
}
