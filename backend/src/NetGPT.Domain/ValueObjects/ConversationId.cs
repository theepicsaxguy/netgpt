// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Domain.ValueObjects
{
    public record ConversationId(Guid Value)
    {
        public static ConversationId CreateNew()
        {
            return new(Guid.NewGuid());
        }

        public static ConversationId From(Guid value)
        {
            return new(value);
        }

        public static ConversationId From(string value)
        {
            return new(Guid.Parse(value));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
