// <copyright file="ConversationId.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Domain.ValueObjects
{
    using System;

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
            return this.Value.ToString();
        }
    }
}
