// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.ValueObjects
{
    using System;

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
            return this.Value.ToString();
        }
    }
}
