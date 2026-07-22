namespace SmartAttendance.Domain.Exceptions;

public class InvalidAddressException : DomainException
{
    public InvalidAddressException(string message) : base(message)
    {
    }
}
