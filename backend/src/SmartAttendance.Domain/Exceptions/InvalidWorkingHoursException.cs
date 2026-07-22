namespace SmartAttendance.Domain.Exceptions;

public class InvalidWorkingHoursException : DomainException
{
    public InvalidWorkingHoursException(string message) : base(message)
    {
    }
}
