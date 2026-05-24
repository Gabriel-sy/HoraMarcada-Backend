using System.Text.RegularExpressions;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Domain.ValueObjects;

public sealed partial record Slug
{
    private static readonly Regex Pattern = SlugPattern();

    public string Value { get; }

    private Slug(string value) => Value = value;

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Slug is required.");

        var normalized = value.Trim().ToLowerInvariant();
        if (!Pattern.IsMatch(normalized))
            throw new DomainValidationException("Slug must contain only lowercase letters, numbers, and hyphens.");

        return new Slug(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();
}
