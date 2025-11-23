using System;

namespace NetGPT.Domain.ValueObjects;

public record ConversationId(Guid Value)
{
    public static ConversationId CreateNew() => new(Guid.NewGuid());
    public static ConversationId From(Guid value) => new(value);
    public static ConversationId From(string value) => new(Guid.Parse(value));
    public override string ToString() => Value.ToString();
}