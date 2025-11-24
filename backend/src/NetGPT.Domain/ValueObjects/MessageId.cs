// Copyright (c) 2025 NetGPT. All rights reserved.

using System;


namespace NetGPT.Domain.ValueObjects
{
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
            return Value.ToString();
        }
    }
}
