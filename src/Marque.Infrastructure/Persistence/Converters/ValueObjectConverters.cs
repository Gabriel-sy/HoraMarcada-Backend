using Marque.Domain.Enums;
using Marque.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Marque.Infrastructure.Persistence.Converters;

internal static class ValueObjectConverters
{
    public static ValueConverter<Slug, string> SlugConverter { get; } = new(
        slug => slug.Value,
        value => Slug.Create(value));

    public static ValueConverter<PhoneNumber, string> PhoneNumberConverter { get; } = new(
        phone => phone.Value,
        value => PhoneNumber.Create(value));

    public static ValueConverter<PhoneNumber?, string?> NullablePhoneNumberConverter { get; } = new(
        phone => phone == null ? null : phone.Value,
        value => PhoneNumber.CreateOptional(value));

    public static ValueConverter<Money, decimal> MoneyConverter { get; } = new(
        money => money.Amount,
        value => Money.Create(value));

    public static ValueConverter<Money?, decimal?> NullableMoneyConverter { get; } = new(
        money => money == null ? null : money.Amount,
        value => Money.CreateOptional(value));

    public static ValueConverter<AppointmentStatus, string> AppointmentStatusConverter { get; } = new(
        status => status.ToString().ToLowerInvariant(),
        value => Enum.Parse<AppointmentStatus>(value, true));
}
