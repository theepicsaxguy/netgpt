using System;

namespace NetGPT.Domain.ValueObjects;

public record UserId(Guid Value)
{
    public static UserId From(Guid value) => new(value);
    public static UserId From(string value) => new(Guid.Parse(value));
    public override string ToString() => Value.ToString();
}