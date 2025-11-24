
namespace NetGPT.Domain.ValueObjects
{
    using System;

    public record MessageId(Guid Value)
    {
        public static MessageId CreateNew()
        {
            return new(Guid.NewGuid());
        }

        public static MessageId From(Guid value)
        {
            return new(value);
        }

        public static MessageId From(string value)
        {
            return new(Guid.Parse(value));
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}
