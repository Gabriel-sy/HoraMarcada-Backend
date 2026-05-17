using System.Text.RegularExpressions;
using Marque.Domain.Exceptions;

namespace Marque.Domain.ValueObjects;

public sealed partial record PhoneNumber
{
    private static readonly Regex Pattern = PhonePattern();

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Phone number is required.");

        var normalized = value.Trim();
        if (!Pattern.IsMatch(normalized))
            throw new DomainValidationException("Phone number must be E.164-friendly (optional +, digits only, 8-15 digits).");

        return new PhoneNumber(normalized);
    }

    public static PhoneNumber? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Create(value);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+?[1-9]\d{7,14}$")]
    private static partial Regex PhonePattern();
}
