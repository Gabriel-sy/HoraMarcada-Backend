using System.Text.RegularExpressions;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Domain.ValueObjects;

public sealed partial record Email
{
    private static readonly Regex Pattern = EmailPattern();

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Email is required.");

        var normalized = value.Trim().ToLowerInvariant();
        if (!Pattern.IsMatch(normalized))
            throw new DomainValidationException("Invalid email format.");

        return new Email(normalized);
    }

    public static Email? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Create(value);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
