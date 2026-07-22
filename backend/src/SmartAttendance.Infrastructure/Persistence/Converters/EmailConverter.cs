using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartAttendance.Domain.ValueObjects;

namespace SmartAttendance.Infrastructure.Persistence.Converters;

/// <summary>
/// Converts the <see cref="Email"/> value object to and from a plain string for EF Core storage.
/// </summary>
public class EmailConverter : ValueConverter<Email, string>
{
    public EmailConverter()
        : base(
            email => email.Value,
            value => new Email(value))
    {
    }
}
