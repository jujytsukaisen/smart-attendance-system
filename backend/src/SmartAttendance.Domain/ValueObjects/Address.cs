using System.Collections.Generic;
using SmartAttendance.Domain.Common;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string Country { get; }
    public string ZipCode { get; }

    public Address(string street, string city, string state, string country, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new InvalidAddressException("Street cannot be empty.");
        if (string.IsNullOrWhiteSpace(city))
            throw new InvalidAddressException("City cannot be empty.");
        if (string.IsNullOrWhiteSpace(state))
            throw new InvalidAddressException("State cannot be empty.");
        if (string.IsNullOrWhiteSpace(country))
            throw new InvalidAddressException("Country cannot be empty.");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new InvalidAddressException("ZipCode cannot be empty.");

        Street = street.Trim();
        City = city.Trim();
        State = state.Trim();
        Country = country.Trim();
        ZipCode = zipCode.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street.ToLowerInvariant();
        yield return City.ToLowerInvariant();
        yield return State.ToLowerInvariant();
        yield return Country.ToLowerInvariant();
        yield return ZipCode.ToLowerInvariant();
    }

    public override string ToString() => $"{Street}, {City}, {State}, {Country} - {ZipCode}";
}
