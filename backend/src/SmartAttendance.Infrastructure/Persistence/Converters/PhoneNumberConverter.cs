using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartAttendance.Domain.ValueObjects;

namespace SmartAttendance.Infrastructure.Persistence.Converters;

/// <summary>
/// Converts the <see cref="PhoneNumber"/> value object to and from a plain string for EF Core storage.
/// </summary>
public class PhoneNumberConverter : ValueConverter<PhoneNumber, string>
{
    public PhoneNumberConverter()
        : base(
            phone => phone.Value,
            value => new PhoneNumber(value))
    {
    }
}
