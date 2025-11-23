using System;

namespace NetGPT.Domain.ValueObjects;

public record MessageId(Guid Value)
{
    public static MessageId CreateNew() => new(Guid.NewGuid());
    public static MessageId From(Guid value) => new(value);
    public static MessageId From(string value) => new(Guid.Parse(value));
    public override string ToString() => Value.ToString();
}