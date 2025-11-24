// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Domain.ValueObjects
{
    public record UserId(Guid Value)
    {
        public static UserId From(Guid value)
        {
            return new(value);
        }

        public static UserId From(string value)
        {
            return new(Guid.Parse(value));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
