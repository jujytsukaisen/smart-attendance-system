using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmartAttendance.Domain.Common;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.ValueObjects;

public class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidEmailException(value);
        }

        var trimmedValue = value.Trim();

        if (!EmailRegex.IsMatch(trimmedValue))
        {
            throw new InvalidEmailException(value);
        }

        Value = trimmedValue;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
