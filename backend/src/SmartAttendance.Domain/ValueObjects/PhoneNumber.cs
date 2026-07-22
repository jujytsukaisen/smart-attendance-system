using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmartAttendance.Domain.Common;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$", 
        RegexOptions.Compiled);

    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidPhoneNumberException(value);
        }

        var cleanValue = Regex.Replace(value.Trim(), @"[\s\-\(\)]", "");

        if (!PhoneRegex.IsMatch(cleanValue))
        {
            throw new InvalidPhoneNumberException(value);
        }

        Value = cleanValue;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
