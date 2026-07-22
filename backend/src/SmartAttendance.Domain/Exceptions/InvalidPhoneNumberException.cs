namespace SmartAttendance.Domain.Exceptions;

public class InvalidPhoneNumberException : DomainException
{
    public InvalidPhoneNumberException(string phoneNumber)
        : base($"The phone number '{phoneNumber}' is invalid.")
    {
    }
}
