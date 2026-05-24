using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new DomainValidationException("Price cannot be negative.");

        return new Money(amount);
    }

    public static Money? CreateOptional(decimal? amount)
    {
        if (amount is null)
            return null;

        return Create(amount.Value);
    }

    public override string ToString() => Amount.ToString("F2");
}
